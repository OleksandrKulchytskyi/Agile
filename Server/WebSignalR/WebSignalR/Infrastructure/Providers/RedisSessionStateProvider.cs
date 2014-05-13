using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Web.SessionState;
using BookSleeve;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.IO;


//https://github.com/chadman/redis-service-provider/blob/master/RedisProvider/SessionProvider/RedisSessionProvider.cs
//https://github.com/Malediction/Booksleeve/blob/master/Tests/Keys.cs
namespace WebSignalR.Infrastructure.Providers
{

	#region Session Item Model
	public class SessionItem
	{

		#region Properties
		public DateTime CreatedAt { get; set; }
		public DateTime LockDate { get; set; }
		public int LockID { get; set; }
		public int Timeout { get; set; }
		public bool Locked { get; set; }
		public string SessionItems { get; set; }
		public int Flags { get; set; }
		#endregion Properties

	}
	#endregion Session Item Model

	public sealed class RedisSessionProvider : System.Web.SessionState.SessionStateStoreProviderBase, IDisposable
	{
		private const int dbId = 1;

		#region Properties
		private string _name;
		private string ApplicationName
		{
			get
			{
				if (string.IsNullOrEmpty(_name))
				{
					if (ConfigurationManager.AppSettings.AllKeys.Contains("Application.Name"))
					{
						_name = ConfigurationManager.AppSettings["Application.Name"];
					}

					_name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
				}

				return _name;
			}
		}

		private RedisConnection RedisSessionClient
		{
			get
			{
				if (!string.IsNullOrEmpty(this.redisPassword))
				{
					return new BookSleeve.RedisConnection(this.redisServer, this.redisPort, password: this.redisPassword);
				}
				return new BookSleeve.RedisConnection(this.redisServer, this.redisPort);
			}
		}

		private RedisConnection RedisSessionAdmin
		{
			get
			{
				if (!string.IsNullOrEmpty(this.redisPassword))
				{
					return new BookSleeve.RedisConnection(this.redisServer, this.redisPort, password: this.redisPassword, allowAdmin: true);
				}
				return new BookSleeve.RedisConnection(this.redisServer, this.redisPort, allowAdmin: true);
			}
		}

		private string redisServer = "localhost";
		private int redisPort = 6379;
		private string redisPassword = string.Empty;
		private SessionStateSection sessionStateConfig = null;
		private bool writeExceptionsToLog = false;

		#endregion Properties

		#region Private Methods
		/// <summary>
		/// Prepends the application name to the redis key if one exists. Querying by application name is recommended for session
		/// </summary>
		/// <param name="id">The session id</param>
		/// <returns>Concatenated string applicationname:sessionkey</returns>
		private string RedisKey(string id)
		{
			return string.Format("{0}{1}", !string.IsNullOrEmpty(this.ApplicationName) ? this.ApplicationName + ":" : "", id);
		}
		#endregion Private Methods

		#region Constructor
		public RedisSessionProvider()
		{

		}
		#endregion Constructor

		#region Overrides
		public override void Dispose()
		{

		}

		public async override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("config");

			if (name == null || name.Length == 0)
				name = "RedisSessionStateStore";

			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "Redis Session State Provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			// Get <sessionState> configuration element.
			Configuration cfg = WebConfigurationManager.OpenWebConfiguration(string.Empty);
			sessionStateConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");


			if (config["writeExceptionsToEventLog"] != null)
				if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
					this.writeExceptionsToLog = true;

			if (config["server"] != null)
				this.redisServer = config["server"];

			if (config["port"] != null)
				int.TryParse(config["port"], out this.redisPort);

			if (config["password"] != null)
			{
				this.redisPassword = config["password"];
				var crypto = Infrastructure.BootStrapper.serviceLocator.Get<WebSignalR.Common.Interfaces.ICrypto>();
				this.redisPassword = crypto.Decrypt(config["password"]);
			}

			using (RedisConnection conn = this.RedisSessionAdmin)
			{
				await conn.Open();

				if (conn.Server.Ping().Result > 0)
					await conn.Server.FlushDb(dbId);

				await conn.CloseAsync(true);
			}
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			return true;
		}

		public async override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			using (RedisConnection client = this.RedisSessionClient)
			{
				await client.Open();
				SessionItem sessionItem = new SessionItem();
				sessionItem.CreatedAt = DateTime.Now.ToUniversalTime();
				sessionItem.LockDate = DateTime.Now.ToUniversalTime();
				sessionItem.LockID = 0;
				sessionItem.Timeout = timeout;
				sessionItem.Locked = false;
				sessionItem.SessionItems = string.Empty;
				sessionItem.Flags = 0;
				//client.Set<SessionItem>(this.RedisKey(id), sessionItem, DateTime.UtcNow.AddMinutes(timeout));
				if (await client.Keys.Exists(dbId, id))
					await client.Keys.Remove(dbId, id);

				await client.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(sessionItem));
				await client.CloseAsync(true);
			}
		}

		public async override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			using (RedisConnection client = this.RedisSessionClient)
			{
				await client.Open();
				// Serialize the SessionStateItemCollection as a string.
				string sessionItems = Serialize((SessionStateItemCollection)item.Items);

				try
				{
					if (newItem)
					{
						SessionItem sessionItem = new SessionItem();
						sessionItem.CreatedAt = DateTime.UtcNow;
						sessionItem.LockDate = DateTime.UtcNow;
						sessionItem.LockID = 0;
						sessionItem.Timeout = item.Timeout;
						sessionItem.Locked = false;
						sessionItem.SessionItems = sessionItems;
						sessionItem.Flags = 0;

						await client.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(sessionItem));
					}
					else
					{
						SessionItem currentSessionItem = GetSessionItem(client, id);
						if (currentSessionItem != null && currentSessionItem.LockID == (int?)lockId)
						{
							currentSessionItem.Locked = false;
							currentSessionItem.SessionItems = sessionItems;
							await client.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentSessionItem));
						}
					}
				}
				catch (Exception e)
				{
					throw e;
				}
			}
		}

		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
		}

		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{
			return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actionFlags);
		}

		private SessionItem GetSessionItem(RedisConnection connection, string id)
		{
			string[] data = connection.Sets.GetAllString(dbId, id).Result;
			if (data != null && data.Length > 0)
				return Newtonsoft.Json.JsonConvert.DeserializeObject<SessionItem>(data[0]);

			return null;
		}

		private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{
			// Initial values for return value and out parameters.
			SessionStateStoreData item = null;
			lockAge = TimeSpan.Zero;
			lockId = null;
			locked = false;
			actionFlags = 0;

			// String to hold serialized SessionStateItemCollection.
			string serializedItems = string.Empty;
			// Timeout value from the data store.
			int timeout = 0;

			using (RedisConnection client = this.RedisSessionClient)
			{
				client.Wait(client.Open());
				try
				{
					if (lockRecord)
					{
						locked = false;
						SessionItem currentItem = GetSessionItem(client, id);
						//SessionItem currentItem = client.Sets.Get<SessionItem>(this.RedisKey(id));

						if (currentItem != null)
						{
							// If the item is locked then do not attempt to update it
							if (!currentItem.Locked)
							{
								currentItem.Locked = true;
								currentItem.LockDate = DateTime.UtcNow;
								client.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentItem));
								//client.Set<SessionItem>(this.RedisKey(id), currentItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
							}
							else
								locked = true;
						}
					}

					SessionItem currentSessionItem = GetSessionItem(client, id);
					//SessionItem currentSessionItem = client.Get<SessionItem>(this.RedisKey(id));

					if (currentSessionItem != null)
					{
						serializedItems = currentSessionItem.SessionItems;
						lockId = currentSessionItem.LockID;
						lockAge = DateTime.UtcNow.Subtract(currentSessionItem.LockDate);
						actionFlags = (SessionStateActions)currentSessionItem.Flags;
						timeout = currentSessionItem.Timeout;
					}
					else
						locked = false;

					if (currentSessionItem != null && !locked)
					{
						// Delete the old item before inserting the new one
						client.Keys.Remove(dbId, id);

						lockId = (int?)lockId + 1;
						currentSessionItem.LockID = lockId != null ? (int)lockId : 0;
						currentSessionItem.Flags = 0;

						client.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentSessionItem));

						// If the actionFlags parameter is not InitializeItem,
						// deserialize the stored SessionStateItemCollection.
						if (actionFlags == SessionStateActions.InitializeItem)
							item = CreateNewStoreData(context, 30);
						else
							item = Deserialize(context, serializedItems, timeout);
					}
				}
				catch (Exception e)
				{
					throw e;
				}
			}
			return item;
		}

		public async override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			using (RedisConnection client = this.RedisSessionClient)
			{
				await client.Open();

				//SessionItem currentSessionItem = client.Get<SessionItem>(this.RedisKey(id));
				SessionItem currentSessionItem = GetSessionItem(client, id);

				if (currentSessionItem != null && (int?)lockId == currentSessionItem.LockID)
				{
					currentSessionItem.Locked = false;
					//client.Set<SessionItem>(this.RedisKey(id), currentSessionItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
					await client.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentSessionItem));
				}

				await client.CloseAsync(true);
			}
		}

		public async override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			using (RedisConnection client = this.RedisSessionClient)
			{
				await client.Open();
				// Delete the old item before inserting the new one
				await client.Keys.Remove(dbId, id);
				await client.CloseAsync(true);
			}
		}

		public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		public async override void ResetItemTimeout(HttpContext context, string id)
		{
			using (RedisConnection client = this.RedisSessionClient)
			{
				await client.Open();
				try
				{
					// TODO :: GET THIS VALUE FROM THE CONFIG
					await client.Keys.Expire(dbId, id, (int)sessionStateConfig.Timeout.TotalSeconds);
					//client.ExpireEntryAt(id, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
				}
				catch (Exception e)
				{
					throw e;
				}
			}
		}

		public override void InitializeRequest(HttpContext context)
		{
			// Was going to open the redis connection here but sometimes I had 5 connections open at one time which was strange
		}

		public override void EndRequest(HttpContext context)
		{
			this.Dispose();
		}

		#endregion Overrides

		#region Serialization
		/// <summary>
		/// Serialize is called by the SetAndReleaseItemExclusive method to
		/// convert the SessionStateItemCollection into a Base64 string to
		/// be stored in MongoDB.
		/// </summary>
		private string Serialize(SessionStateItemCollection items)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(ms))
			{
				if (items != null)
					items.Serialize(writer);
				writer.Close();
				return Convert.ToBase64String(ms.ToArray());
			}
		}

		private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
		{
			using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(serializedItems)))
			{
				SessionStateItemCollection sessionItems = new SessionStateItemCollection();

				if (ms.Length > 0)
				{
					using (BinaryReader reader = new BinaryReader(ms))
					{
						sessionItems = SessionStateItemCollection.Deserialize(reader);
					}
				}

				return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);
			}
		}
		#endregion Serialization
	}
}