using AutoMapper;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;
using WebSignalR.Infrastructure;

namespace WebSignalR.Hubs
{
	[Microsoft.AspNet.SignalR.Hubs.HubName("testhub")]
	public class TestHub : Hub, IRoomService, IVoteService
	{
		private static readonly Version _version = typeof(TestHub).Assembly.GetName().Version;
		private static readonly string _versionString = _version.ToString();
		private readonly IUnityOfWork _unity;
		private readonly ICryptoService _cryptoService;

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

		public override Task OnConnected()
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

		public override Task OnDisconnected()
		{
			DisconnectClient(Context.ConnectionId);

			return base.OnDisconnected();
		}

		public override Task OnReconnected()
		{
			if (!Context.User.Identity.IsAuthenticated)
				return null;
			Clients.Caller.onStatus("Reconnecting...").Wait();

			return base.OnReconnected();
		}

		private void SetStateData(string sate)
		{
			Clients.Caller.version = _versionString;
			Clients.Caller.state = sate;
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
			if (Context.User.Identity.IsAuthenticated)
			{
				IRepository<User> repo = _unity.GetRepository<User>();
				User usr = repo.Get(x => x.Name == Context.User.Identity.Name).FirstOrDefault();
				if (usr != null)
				{
					IRepository<UserSession> repoSession = _unity.GetRepository<UserSession>();
					UserSession us = repoSession.Get(x => x.SessionId == clientId).FirstOrDefault();
					if (us != null)
					{
						repoSession.Remove(us);
						_unity.Commit();
					}
				}
			}
			#region commented
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
			#endregion
		}

		[Infrastructure.Authorization.SignalRAuth(Roles = "User")]
		public JoinRoomResult JoinRoom(string roomName, string sessionId)
		{
			JoinRoomResult result = new JoinRoomResult();
			Task addTask = Groups.Add(Context.ConnectionId, roomName);
			IReadOnlyRepository<User> userRepo = _unity.GetRepository<User>();
			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room room = repo.Get(x => x.Name == roomName).FirstOrDefault();
			User usr = userRepo.Get(x => x.ConnectedSessions.Any(s => s.SessionId == sessionId)).FirstOrDefault();
			if (room != null && userRepo != null)
			{
				result.Active = room.Active;
				result.HostMaster = room.Active;
				room.ConnectedUsers.Add(usr);
				_unity.Commit();
			}
			RoomDto dto = Mapper.Map<RoomDto>(room);
			Clients.Group(roomName, Context.ConnectionId).onJoinRoom(dto);
			return result;
		}

		[Infrastructure.Authorization.SignalRAuth(Roles = "User")]
		public Task LeaveRoom(string roomName, string sessionId)
		{
			Task addTask = Groups.Remove(Context.ConnectionId, roomName);
			IReadOnlyRepository<User> userRepo = _unity.GetRepository<User>();
			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room room = repo.Get(x => x.Name == roomName).FirstOrDefault();
			User usr = userRepo.Get(x => x.ConnectedSessions.Any(s => s.SessionId == sessionId)).FirstOrDefault();
			if (room != null && userRepo != null)
			{
				room.ConnectedUsers.Remove(usr);
				_unity.Commit();
			}

			RoomDto dto = Mapper.Map<RoomDto>(room);
			Clients.Group(roomName).onLeaveRoom(dto);
			return addTask;
		}

		[Infrastructure.Authorization.SignalRAuth(Roles = "ScrumMaster")]
		public Task ChangeRoomState(string roomName, bool state)
		{
			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room room = repo.Get(x => x.Name == roomName).FirstOrDefault();
			if (room != null)
			{
				room.Active = state;
				_unity.Commit();
			}
			return Clients.Group(roomName).onRoomStateChanged(roomName, state);
		}

		[Infrastructure.Authorization.SignalRAuth(Roles = "User")]
		public Task VoteForItem(string room, int voteItemId, int userId, int mark)
		{
			IRepository<VoteItem> repo = _unity.GetRepository<VoteItem>();
			IRepository<User> repoUser = _unity.GetRepository<User>();
			IRepository<UserVote> userVorePero = _unity.GetRepository<UserVote>();

			VoteItem vote = repo.Get(x => x.Id == voteItemId).FirstOrDefault();
			User usr = repoUser.Get(x => x.Id == userId).FirstOrDefault();
			UserVoteDto dto = null;
			if (vote != null && usr != null)
			{
				UserVote uv = new UserVote();
				uv.UserId = usr.Id;
				uv.VoteId = vote.Id;
				uv.Mark = mark;
				userVorePero.Add(uv);
				_unity.Commit();
				vote.VotedUsers.Add(uv);
				dto = Mapper.Map<UserVoteDto>(uv);
			}

			//VoteItemDto voteDto = Mapper.Map<VoteItemDto>(vote);
			return Clients.Group(room).onUserVoted(dto);
		}

		[Infrastructure.Authorization.SignalRAuth(Roles = "ScrumMaster")]
		public Task CloseVote(string room, int voteItemId, int mark)
		{
			IRepository<VoteItem> repo = _unity.GetRepository<VoteItem>();
			VoteItem vote = repo.Get(x => x.Id == voteItemId).FirstOrDefault();
			if (vote != null)
			{
				vote.OverallMark = mark;
				vote.Closed = true;
				vote.VotedUsers.Clear();
				_unity.Commit();
			}
			VoteItemDto voteDto = Mapper.Map<VoteItemDto>(vote);

			return Clients.Group(room).onVoteItemClosed(voteDto);
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}