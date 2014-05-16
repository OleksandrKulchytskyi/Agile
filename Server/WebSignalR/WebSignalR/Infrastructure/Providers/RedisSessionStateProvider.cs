using BookSleeve;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;


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
		private class RedisConnectionFactory
		{
			public RedisConnection Get(string server, int port, string password, bool admin)
			{
				if (admin)
				{
					if (!string.IsNullOrEmpty(password))
					{
						return new BookSleeve.RedisConnection(server, port, password: password, allowAdmin: true);
					}
					return new BookSleeve.RedisConnection(server, port, allowAdmin: true);
				}
				else
				{
					if (!string.IsNullOrEmpty(password))
					{
						return new BookSleeve.RedisConnection(server, port, password: password);
					}
					return new BookSleeve.RedisConnection(server, port);
				}
			}
		}

		private const int dbId = 1;

		#region Properties

		private RedisConnectionFactory _factory;
		private RedisConnection _connection;

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
				return _factory.Get(this.redisServer, this.redisPort, this.redisPassword, false);
			}
		}

		private RedisConnection RedisSessionAdmin
		{
			get
			{
				return _factory.Get(this.redisServer, this.redisPort, this.redisPassword, true);
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
			_factory = new RedisConnectionFactory();
		}
		#endregion Constructor

		#region Overrides
		public override void Dispose()
		{
			if (_connection != null)
			{
				_connection.Error -= OnConnectionError;
				_connection.Close(true);
				_connection.Dispose();
			}
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

			if (config["host"] != null)
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
			SessionItem sessionItem = new SessionItem();
			sessionItem.CreatedAt = DateTime.Now.ToUniversalTime();
			sessionItem.LockDate = DateTime.Now.ToUniversalTime();
			sessionItem.LockID = 0;
			sessionItem.Timeout = timeout;
			sessionItem.Locked = false;
			sessionItem.SessionItems = string.Empty;
			sessionItem.Flags = 0;

			//client.Set<SessionItem>(this.RedisKey(id), sessionItem, DateTime.UtcNow.AddMinutes(timeout));
			if (await _connection.Keys.Exists(dbId, id))
				await _connection.Keys.Remove(dbId, id);

			await _connection.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(sessionItem));
		}

		public async override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
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

					await _connection.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(sessionItem));
				}
				else
				{
					SessionItem currentSessionItem = GetSessionItem(_connection, id);
					if (currentSessionItem != null && currentSessionItem.LockID == (int?)lockId)
					{
						currentSessionItem.Locked = false;
						currentSessionItem.SessionItems = sessionItems;
						await _connection.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentSessionItem));
					}
				}
			}
			catch (Exception e)
			{
				Global.Logger.Error(e);
				throw e;
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
			//connection.s
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

			try
			{
				if (lockRecord)
				{
					locked = false;
					SessionItem currentItem = GetSessionItem(_connection, id);
					//SessionItem currentItem = client.Sets.Get<SessionItem>(this.RedisKey(id));

					if (currentItem != null)
					{
						// If the item is locked then do not attempt to update it
						if (!currentItem.Locked)
						{
							currentItem.Locked = true;
							currentItem.LockDate = DateTime.UtcNow;
							_connection.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentItem));
							//client.Set<SessionItem>(this.RedisKey(id), currentItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
						}
						else
							locked = true;
					}
				}

				SessionItem currentSessionItem = GetSessionItem(_connection, id);
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
					_connection.Keys.Remove(dbId, id);

					lockId = (int?)lockId + 1;
					currentSessionItem.LockID = lockId != null ? (int)lockId : 0;
					currentSessionItem.Flags = 0;

					_connection.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentSessionItem));

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
				Global.Logger.Error(e);
				throw e;
			}

			return item;
		}

		public async override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			//SessionItem currentSessionItem = client.Get<SessionItem>(this.RedisKey(id));
			SessionItem currentSessionItem = GetSessionItem(_connection, id);

			if (currentSessionItem != null && (int?)lockId == currentSessionItem.LockID)
			{
				currentSessionItem.Locked = false;
				//client.Set<SessionItem>(this.RedisKey(id), currentSessionItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
				await _connection.Sets.Add(dbId, id, Newtonsoft.Json.JsonConvert.SerializeObject(currentSessionItem));
			}
		}

		public async override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			// Delete the old item before inserting the new one
			await _connection.Keys.Remove(dbId, id);
		}

		public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		public async override void ResetItemTimeout(HttpContext context, string id)
		{
			try
			{
				// TODO :: GET THIS VALUE FROM THE CONFIG
				await _connection.Keys.Expire(dbId, id, (int)sessionStateConfig.Timeout.TotalSeconds);
				//client.ExpireEntryAt(id, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public async override void InitializeRequest(HttpContext context)
		{
			_connection = RedisSessionClient;
			_connection.Error += OnConnectionError;
			await _connection.Open();
		}

		public override void EndRequest(HttpContext context)
		{
			this.Dispose();
		}

		#endregion Overrides

		private void OnConnectionError(object sender, BookSleeve.ErrorEventArgs e)
		{
			RedisConnection con = (sender as RedisConnection);
			if (con != null)
				con.Error -= OnConnectionError;

			Global.Logger.Error("OnConnectionError - " + e.Cause + ". " + e.Exception.GetBaseException());

			//AttemptReconnect(e.Exception);
		}

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