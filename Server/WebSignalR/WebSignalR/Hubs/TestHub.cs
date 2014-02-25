using AutoMapper;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Infrastructure;

namespace WebSignalR.Hubs
{
	[Microsoft.AspNet.SignalR.Hubs.HubName("testhub")]
	public class TestHub : Hub
	{
		private static readonly Version _version = typeof(TestHub).Assembly.GetName().Version;
		private static readonly string _versionString = _version.ToString();
		private IUnityOfWork _unity;
		private ICryptoService _cryptoService;

		private string UserAgent
		{
			get
			{
				if (Context.Headers != null)
					return Context.Headers["User-Agent"];
				return null;
			}
		}

		private bool OutOfSync
		{
			get
			{
				string version = Clients.Caller.version;
				return string.IsNullOrEmpty(version) || new Version(version) != _version;
			}
		}

		public TestHub(IUnityOfWork unity, ICryptoService crypto)
		{
			_unity = unity;
			_cryptoService = crypto;
		}

		public override System.Threading.Tasks.Task OnConnected()
		{
			if (Context.User.Identity.IsAuthenticated)
			{
				IRepository<User> repo = _unity.GetRepository<User>();
				User usr = repo.Get(x => x.Name == Context.User.Identity.Name).FirstOrDefault();
				if (usr != null)
				{
					IRepository<UserSession> repoSession = _unity.GetRepository<UserSession>();
					UserDto dto = Mapper.Map<UserDto>(usr);
					Clients.Caller.onUserLogged(dto);
					UserSession us = new UserSession();
					us.User = usr;
					us.SessionId = this.Context.ConnectionId;
					us.UserAgent = UserAgent;
					us.UserId = usr.Id;
					us.SetLastActivityNow();
					repoSession.Add(us);
					_unity.Commit();
					SessionState state = new SessionState() { UserId = usr.Id, SessionId = us.SessionId };
					string jsonState = JsonConvert.SerializeObject(state);
					SetStateData(jsonState);
					Clients.Caller.onState(jsonState);
				}
			}

			return base.OnConnected();
		}

		private void SetStateData(string sate)
		{
			Clients.Caller.version = _versionString;
			Clients.Caller.state = sate;
		}

		public override System.Threading.Tasks.Task OnDisconnected()
		{
			if (Context.User.Identity.IsAuthenticated)
			{
				IRepository<User> repo = _unity.GetRepository<User>();
				User usr = repo.Get(x => x.Name == Context.User.Identity.Name).FirstOrDefault();
				if (usr != null)
				{
					IRepository<UserSession> repoSession = _unity.GetRepository<UserSession>();
					UserSession us = repoSession.Get(x => x.SessionId == Context.ConnectionId).FirstOrDefault();
					if (us != null)
					{
						repoSession.Remove(us);
						_unity.Commit();
					}
				}
			}

			return base.OnDisconnected();
		}

		public override System.Threading.Tasks.Task OnReconnected()
		{
			if (!Context.User.Identity.IsAuthenticated)
				return null;
			Clients.Caller.onStatus("Reconnecting...").Wait();

			return base.OnReconnected();
		}

		private SessionState GetSessionState()
		{
			var userState = GetCookieValue("user.state");
			SessionState clientState = null;

			if (string.IsNullOrEmpty(userState))
				clientState = new SessionState(); //initializing default client state
			else
			{
				try
				{
					clientState = JsonConvert.DeserializeObject<SessionState>(userState);
				}
				catch (JsonException ex)
				{
					Global.Logger.Error("Hub -> GetSessionState", ex);
				}
			}
			// Read the id from the caller if there's no cookie
			clientState.UserId = clientState.UserId ?? Clients.Caller.uid;
			return clientState;
		}

		private string GetCookieValue(string key)
		{
			if (Context == null || !Context.RequestCookies.ContainsKey(key))
				return null;
			var cookie = Context.RequestCookies[key];
			string value = cookie != null ? cookie.Value : null;
			return value != null ? HttpUtility.UrlDecode(value) : null;
		}

		private void DisconnectClient(string clientId)
		{
			//string userId = _service.DisconnectClient(clientId);

			//if (string.IsNullOrEmpty(userId))
			//	return;

			//// Query for the user to get the updated status
			//MessangerUser user = _repository.GetUserById(userId);

			//// There's no associated user for this client id
			//if (user == null)
			//	return;

			//// The user will be marked as offline if all clients leave
			//if (user.Status == (int)UserStatus.Offline)
			//{
			//	foreach (var group in user.Groups)
			//	{
			//		var userViewModel = new UserViewModel(user);
			//		//Clients.Group(group.Name).leave(userViewModel, group.Name).Wait();
			//		Groups.Remove(clientId, group.Name);
			//	}
			//}
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();

			base.Dispose(disposing);
		}
	}
}