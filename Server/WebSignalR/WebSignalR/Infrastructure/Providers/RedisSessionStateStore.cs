using BookSleeve;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace WebSignalR.Infrastructure.Providers
{
	public sealed class RedisSessionStateStore : SessionStateStoreProviderBase
	{
		private RedisConnection redisConnection;

		private string lockHashKey;
		private readonly object locker = new { };

		private int port;
		private int sessionTimeout;
		private string host;
		private string password;

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				name = "AspNetSession";
			}
			base.Initialize(name, config);

			// Get <sessionState> configuration element.
			Configuration cfg = WebConfigurationManager.OpenWebConfiguration(string.Empty);
			SessionStateSection sessionStateConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");

			if (config["host"] != null)
				this.host = config["host"];

			if (config["port"] != null)
				int.TryParse(config["port"], out this.port);

			if (config["password"] != null)
			{
				this.password = config["password"];
				var crypto = Infrastructure.BootStrapper.serviceLocator.Get<WebSignalR.Common.Interfaces.ICrypto>();
				this.password = crypto.Decrypt(config["password"]);
			}

			if (config["timeout"] != null)
				int.TryParse(config["timeout"], out this.sessionTimeout);
			else
				sessionTimeout = 60;

			lockHashKey = name + ":LockedSessions";
		}

		private RedisConnection GetRedisConnection()
		{
			if (redisConnection == null ||
				 (redisConnection.State != RedisConnectionBase.ConnectionState.Open &&
				  redisConnection.State != RedisConnectionBase.ConnectionState.Opening))
			{
				lock (locker)
				{
					if (redisConnection == null ||
						 (redisConnection.State != RedisConnectionBase.ConnectionState.Open &&
						  redisConnection.State != RedisConnectionBase.ConnectionState.Opening))
					{
						redisConnection = new RedisConnection(this.host, this.port, password: this.password);
						redisConnection.Closed += redisConnection_Closed;
						redisConnection.Open();
					}
				}
			}
			return redisConnection;
		}

		private void redisConnection_Closed(object sender, EventArgs e)
		{
			RedisConnection connection = (sender as RedisConnection);
			if (connection != null)
				connection.Error -= redisConnection_Closed;
		}

		private string GetKeyForSessionId(string id)
		{
			return this.Name + ":" + id;
		}

		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			var redis = GetRedisConnection();
			var getLock = redis.Hashes.Get(0, lockHashKey, id);
			var lockIdAsBytes = (byte[])lockId;
			byte[] sessionData = null;
			using (var ms = new MemoryStream())
			{
				var writer = new BinaryWriter(ms);

				if (item.Items as SessionStateItemCollection != null)
					((SessionStateItemCollection)item.Items).Serialize(writer);

				writer.Close();
				sessionData = ms.ToArray();
			}
			var sessionItemHash = new Dictionary<string, byte[]>();
			sessionItemHash.Add("initialize", new byte[] { 0 });
			sessionItemHash.Add("data", sessionData);
			sessionItemHash.Add("timeoutMinutes", BitConverter.GetBytes(item.Timeout));

			LockData lockData;
			getLock.Wait();
			if (getLock.Result == null)
			{
				redis.Hashes.Set(0, GetKeyForSessionId(id), sessionItemHash, false);
			}
			else if (LockData.TryParse(getLock.Result, out lockData) && Enumerable.SequenceEqual(lockData.LockId, lockIdAsBytes))
			{
				redis.Hashes.Set(0, GetKeyForSessionId(id), sessionItemHash, false);
				redis.Hashes.Remove(0, lockHashKey, id);
			}
		}

		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			var redis = GetRedisConnection();
			var getSessionData = redis.Hashes.GetAll(0, GetKeyForSessionId(id));
			locked = false;
			lockAge = new TimeSpan(0);
			lockId = null;
			actions = SessionStateActions.None;

			if (getSessionData.Result == null)
			{
				return null;
			}
			else
			{
				var sessionItems = new SessionStateItemCollection();
				var sessionDataHash = getSessionData.Result;
				if (sessionDataHash.Count == 3)
				{
					using (var ms = new MemoryStream(sessionDataHash["data"]))
					{
						if (ms.Length > 0)
						{
							var reader = new BinaryReader(ms);
							sessionItems = SessionStateItemCollection.Deserialize(reader);
						}
					}
					var timeoutMinutes = BitConverter.ToInt32(sessionDataHash["timeoutMinutes"], 0);
					redis.Keys.Expire(0, GetKeyForSessionId(id), timeoutMinutes * 60);
				}

				return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), sessionTimeout);
			}
		}

		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			var redis = GetRedisConnection();
			var getLockData = redis.Hashes.Get(0, lockHashKey, id);

			actions = SessionStateActions.None;
			locked = false;
			lockId = null;
			lockAge = TimeSpan.MinValue;

			getLockData.Wait();
			byte[] rawLockData = getLockData.Result;

			if (rawLockData == null)
			{
				var lockData = LockData.New();
				using (var trans = redis.CreateTransaction())
				{
					var setLock = trans.Hashes.SetIfNotExists(0, lockHashKey, id, lockData.ToByteArray());
					var getSessionData = trans.Hashes.GetAll(0, GetKeyForSessionId(id));
					trans.Execute();
					setLock.Wait();
					if (setLock.Result)
					{
						locked = true;
						lockAge = new TimeSpan(0);
						lockId = lockData.LockId;

						var sessionItems = new SessionStateItemCollection();
						var sessionDataHash = redis.Wait(getSessionData);
						if (sessionDataHash.Count == 3)
						{
							actions = sessionDataHash["initialize"][0] == 1 ? SessionStateActions.InitializeItem : SessionStateActions.None;

							using (var ms = new MemoryStream(sessionDataHash["data"]))
							{
								if (ms.Length > 0)
								{
									var reader = new BinaryReader(ms);
									sessionItems = SessionStateItemCollection.Deserialize(reader);
								}
							}

							var timeoutMinutes = BitConverter.ToInt32(sessionDataHash["timeoutMinutes"], 0);
							redis.Keys.Expire(0, GetKeyForSessionId(id), timeoutMinutes * 60);
						}
						return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), sessionTimeout);
					}
					else
					{
						rawLockData = redis.Hashes.Get(0, lockHashKey, id).Result;
						if (rawLockData != null && LockData.TryParse(rawLockData, out lockData))
						{
							locked = true;
							lockId = lockData.LockId;
							lockAge = DateTime.UtcNow - lockData.LockUtcTime;
						}
						return null;
					}
				}
			}
			else
			{
				LockData lockData;
				if (LockData.TryParse(rawLockData, out lockData))
				{
					locked = true;
					lockId = lockData.LockId;
					lockAge = DateTime.UtcNow - lockData.LockUtcTime;
				}
				return null;
			}
		}

		internal struct LockData
		{
			static readonly byte SEPERATOR = Encoding.ASCII.GetBytes(";")[0];

			public static LockData New()
			{
				var data = new LockData();
				data.LockId = Guid.NewGuid().ToByteArray();
				data.LockUtcTime = DateTime.UtcNow;
				return data;
			}

			public static bool TryParse(byte[] raw, out LockData data)
			{
				if (raw != null && raw.Length > 1)
				{
					var lockId = raw.Take(16).ToArray();
					var lockTicks = BitConverter.ToInt64(raw, 17);
					data = new LockData { LockId = lockId, LockUtcTime = new DateTime(lockTicks) };
					return true;
				}
				data = new LockData();
				return false;
			}

			public byte[] LockId;
			public DateTime LockUtcTime;

			public override string ToString()
			{
				return BitConverter.ToString(LockId) + ";" + LockUtcTime.Ticks;
			}

			public byte[] ToByteArray()
			{
				return LockId.Concat(new byte[] { SEPERATOR }.Concat(BitConverter.GetBytes(LockUtcTime.Ticks))).ToArray();
			}
		}

		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			var redis = GetRedisConnection();
			byte[] sessionData = null;
			using (var ms = new MemoryStream())
			{
				var writer = new BinaryWriter(ms);
				(new SessionStateItemCollection()).Serialize(writer);
				writer.Close();
				sessionData = ms.ToArray();
			}
			var newItemHash = new Dictionary<string, byte[]>();
			newItemHash.Add("data", sessionData);
			newItemHash.Add("initialize", new byte[] { 0 });
			newItemHash.Add("timeoutMinutes", BitConverter.GetBytes(timeout));
			redis.Hashes.Set(0, GetKeyForSessionId(id), newItemHash, false);
			redis.Keys.Expire(0, GetKeyForSessionId(id), timeout * 60);
		}

		public override void Dispose()
		{
			var redis = GetRedisConnection();
			IDisposable disposable = null;
			if ((disposable = redis as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}

		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			var redis = GetRedisConnection();
			//var getLock = redis.Hashes.Get(0, lockHashKey, id);
			//var lockIdAsString = (string)lockId;
			//if (getLock.Result && getLock.Result == lockIdAsString) {
			redis.Hashes.Remove(0, lockHashKey, id);
			//}
		}

		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			var redis = GetRedisConnection();
			var getLock = redis.Hashes.Get(0, lockHashKey, id);
			var lockIdAsBytes = (byte[])lockId;
			LockData lockData;
			if (getLock.Result != null && LockData.TryParse(getLock.Result, out lockData) && Enumerable.SequenceEqual(lockData.LockId, lockIdAsBytes))
			{
				redis.Keys.Remove(0, GetKeyForSessionId(id));
				redis.Hashes.Remove(0, lockHashKey, id);
			}
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}

		public override void ResetItemTimeout(HttpContext context, string id)
		{
			var redis = GetRedisConnection();
			var getTimeoutMinutes = redis.Hashes.Get(0, GetKeyForSessionId(id), "timeoutMinutes");

			if (getTimeoutMinutes.Result != null)
			{
				var timeoutMinutes = BitConverter.ToInt32(getTimeoutMinutes.Result, 0);
				redis.Keys.Expire(0, GetKeyForSessionId(id), timeoutMinutes * 60);
			}
		}

		public override void InitializeRequest(HttpContext context)
		{
		}

		public override void EndRequest(HttpContext context)
		{
		}
	}
}