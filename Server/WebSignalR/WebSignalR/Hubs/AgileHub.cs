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
using WebSignalR.Infrastructure.Authorization;

namespace WebSignalR.Hubs
{
	[Microsoft.AspNet.SignalR.Hubs.HubName("agileHub")]
	[SignalRAuth]
	public class AgileHub : Hub, IRoomService, IVoteService
	{
		private static readonly Version _version = typeof(AgileHub).Assembly.GetName().Version;
		private static readonly string _versionString = _version.ToString();

		private readonly IUnityOfWork _unity;
		private readonly ICryptoService _cryptoService;
		private readonly IUserRoomService _roomService;
		private readonly ISessionService _sessionServ;

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

		public AgileHub(IUnityOfWork unity, ICryptoService crypto, IUserRoomService roomService, ISessionService sessionServ)
		{
			_unity = unity;
			_cryptoService = crypto;
			_roomService = roomService;
			_sessionServ = sessionServ;
		}

		public override Task OnConnected()
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine("OnConnected user: " + Context.User.Identity.Name + " SessionId: " + Context.ConnectionId);
#endif
			if (Context.User.Identity.IsAuthenticated)
			{
				IRepository<User> repo = GetRepository<User>();
				User usr = repo.Get(x => x.Name == Context.User.Identity.Name).FirstOrDefault();
				if (usr != null)
				{
					IRepository<UserSession> repoSession = GetRepository<UserSession>();
					UserDto dto = Mapper.Map<UserDto>(usr);
					Clients.Caller.onUserLogged(dto);
					UserSession us = new UserSession();
					us.User = usr;
					us.SessionId = this.Context.ConnectionId;
					SessionState state = new SessionState() { UserId = usr.Id, SessionId = us.SessionId };
					SetStateData(JsonConvert.SerializeObject(state));
					Clients.Caller.onState(state);
					us.UserAgent = UserAgent;
					us.UserId = usr.Id;
					us.SetLastActivityNow();
					repoSession.Add(us);
					_unity.Commit();
				}
			}

			return base.OnConnected();
		}

		public override Task OnDisconnected()
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine("OnDisconnected user: " + Context.User.Identity.Name + " SessionId: " + Context.ConnectionId);
#endif
			DisconnectClient(Context.ConnectionId);
			return base.OnDisconnected();
		}

		public override Task OnReconnected()
		{
			if (!Context.User.Identity.IsAuthenticated)
				return null;

			try
			{
				_sessionServ.UpdateSessionActivity(Context.ConnectionId);
			}
			catch (Exception ex) { Global.Logger.Error(ex); }
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
				IRepository<UserSession> repoSession = GetRepository<UserSession>();
				IRepository<SessionRoom> srRepo = GetRepository<SessionRoom>();
				UserSession us = _sessionServ.GetCurrentSession(Context.User.Identity.Name, Context.ConnectionId);
				if (us != null)
				{
					SessionRoom sr = srRepo.Get(x => x.Id == us.Id).FirstOrDefault();
					if (sr != null)
					{
						UserDto uDto = Mapper.Map<UserDto>(us.User);
						this.Clients.Group(sr.RoomName).onLeftRoom(uDto);
						srRepo.Remove(us.SessionRoom);
					}

					repoSession.Remove(us);
					_unity.Commit();
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

			#endregion commented
		}

		[SignalRAuth(Roles = "User")]
		public Task JoinRoom(string roomName, string sessionId)
		{
			Guid sesID;
			if (!Guid.TryParse(sessionId, out sesID))
				sessionId = Context.ConnectionId;

			try
			{
				Room room = _roomService.JoinToRoomBySessionId(roomName, sessionId);
				Task addTask = Groups.Add(sessionId, roomName);
				RoomDto rDto = Mapper.Map<RoomDto>(room);
				Clients.Caller.onInitRoom(rDto);

				_sessionServ.UpdateSessionActivity(sessionId);
				var usRepo = GetRepository<UserSession>();

				UserSession us = usRepo.Get(x => x.SessionId == sessionId).FirstOrDefault();
				if (us.User != null)
				{
					SessionRoom sr = new SessionRoom();
					sr.SetLoggedInNow();
					sr.RoomName = roomName;
					us.SessionRoom = sr;
					usRepo.Update(us);
					_unity.Commit();

					UserDto uDto = Mapper.Map<UserDto>(us.User);
					return Clients.Group(roomName, Context.ConnectionId).onJoinedRoom(uDto);
				}
			}
			catch (Exception ex)
			{
				Clients.Caller.onErrorHandler(ex.Message);
				throw;
			}
			return EmptyTask;
		}

		[SignalRAuth(Roles = "User")]
		public Task LeaveRoom(string roomName, string sessionId)
		{
			Guid sesID;
			if (!Guid.TryParse(sessionId, out sesID))
				sessionId = Context.ConnectionId;

			Task removeTask = Groups.Remove(sessionId, roomName);

			_sessionServ.UpdateSessionActivity(sessionId);
			Room room = _roomService.DisconnecFromRoomBySessionId(roomName, sessionId);
			var usRepo = GetRepository<UserSession>();
			var srRepo = GetRepository<SessionRoom>();
			UserSession us = usRepo.Get(x => x.SessionId == sessionId).FirstOrDefault();
			if (us != null)
			{
				SessionRoom sr = srRepo.Get(x => x.Id == us.Id).FirstOrDefault();
				if (sr != null)
				{
					srRepo.Remove(us.SessionRoom);
					_unity.Commit();
				}
			}

			UserDto uDto = Mapper.Map<UserDto>(GetRepository<UserSession>().Get(x => x.SessionId == sessionId).Select(x => x.User).FirstOrDefault());
			Clients.Group(roomName).onLeftRoom(uDto);

			return removeTask;
		}

		[SignalRAuth(Roles = "ScrumMaster")]
		public Task ChangeRoomState(string roomName, bool state)
		{
			_roomService.ChangeRoomState(roomName, state);
			Room room = GetRepository<Room>().Get(x => x.Name == roomName).FirstOrDefault();
			IRepository<VoteItem> voteRepo = GetRepository<VoteItem>();

			if (room.ItemsToVote != null && state)//in case whe we open room clear all previous user votes
			{
				foreach (VoteItem item in room.ItemsToVote)
				{
					item.Opened = false;
					item.Closed = false;
					if (item.VotedUsers != null)
						item.VotedUsers.Clear();
					voteRepo.Update(item);
				}
			}
			try { _unity.Commit(); }
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				Clients.Caller.onErrorHandler(ex.Message);
			}

			RoomDto dto = Mapper.Map<RoomDto>(room);
			return Clients.Group(roomName).onRoomStateChanged(dto);
		}

		[SignalRAuth(Roles = "ScrumMaster")]
		public Task AddVote(string roomName, string content)
		{
			IRepository<Room> roomRepo = GetRepository<Room>();
			IRepository<VoteItem> voteRepo = GetRepository<VoteItem>();

			Room room = roomRepo.Get(x => x.Name == roomName).FirstOrDefault();
			VoteItem vote = voteRepo.Get(v => v.Content == content).FirstOrDefault();
			if (room != null && vote == null)
			{
				vote = new VoteItem();
				vote.Closed = false;
				vote.HostRoom = room;
				vote.Content = content;
				vote.OverallMark = 0;
				try
				{
					room.ItemsToVote.Add(vote);
					roomRepo.Update(room);
					_unity.Commit();
				}
				catch (Exception ex)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("Error occurred: AddVote" + Environment.NewLine + ex.ToString());
#endif
					throw;
				}

				return Clients.Group(room.Name).onRoomStateChanged(Mapper.Map<RoomDto>(room));
			}
			return EmptyTask;
		}

		[SignalRAuth(Roles = "ScrumMaster")]
		public Task RemoveVote(string roomName, int voteId)
		{
			IRepository<Room> roomRepo = GetRepository<Room>();
			IRepository<VoteItem> voteRepo = GetRepository<VoteItem>();

			Room room = roomRepo.Get(x => x.Name == roomName).FirstOrDefault();
			VoteItem vote = voteRepo.Get(v => v.Id == voteId).FirstOrDefault();
			if (room != null && vote != null)
			{
				try
				{
					room.ItemsToVote.Remove(vote);
					roomRepo.Update(room);
					_unity.Commit();
				}
				catch (Exception ex)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("Error occurred: RemoveVote" + Environment.NewLine + ex.ToString());
#endif
					throw;
				}

				return Clients.Group(room.Name).onRoomStateChanged(Mapper.Map<RoomDto>(room));
			}
			return EmptyTask;
		}

		[SignalRAuth(Roles = "User")]
		public Task VoteForItem(string room, int voteItemId, int mark)
		{
			IRepository<VoteItem> repo = GetRepository<VoteItem>();
			IRepository<User> repoUser = GetRepository<User>();
			IRepository<UserVote> userVorePero = GetRepository<UserVote>();

			VoteItem voteItem = repo.Get(x => x.Id == voteItemId).FirstOrDefault();
			User usr = repoUser.Get(x => x.Name == Context.User.Identity.Name).FirstOrDefault();
			UserVoteDto userVoteDto = null;
			if (voteItem != null && usr != null && voteItem.Opened)
			{
				UserVote uv = new UserVote();
				uv.UserId = usr.Id;
				uv.VoteId = voteItem.Id;
				uv.Mark = mark;
				userVorePero.Add(uv);

				voteItem.VotedUsers.Add(uv);
				_unity.Commit();
				_sessionServ.UpdateSessionActivity(Context.ConnectionId);
				userVoteDto = Mapper.Map<UserVoteDto>(uv);
				Task resultTask = Clients.Group(room).onUserVoted(userVoteDto);
				if (voteItem.VotedUsers.Count == voteItem.HostRoom.ConnectedUsers.Count) //TODO: check logic related to the vote finished event here 
				{
					Clients.Group(room).onVoteFinished(voteItem);
				}
				return resultTask;
			}
			else
			{
				if (voteItem != null && !voteItem.Opened)
					return Clients.Caller.onErrorHandler("Cannot submit vote for non-opend vote item.");
			}

			return EmptyTask;
		}

		[SignalRAuth(Roles = "ScrumMaster")]
		public Task OpenVoteItem(string room, int voteItemId)
		{
			IRepository<VoteItem> repo = GetRepository<VoteItem>();
			VoteItem voteItem = repo.Get(x => x.Id == voteItemId).FirstOrDefault();
			if (voteItem != null)
			{
				voteItem.Opened = true;
				voteItem.Closed = false;
				if (voteItem.VotedUsers.Count > 0)
				{
					IRepository<UserVote> repoVotes = GetRepository<UserVote>();//fixing the issue related to foreign key missing exception in case of voteItem.VotedUsers.Clear(); be only invoked.
					foreach (var vote in voteItem.VotedUsers)
					{
						repoVotes.Remove(vote);
					}
					voteItem.VotedUsers.Clear();
				}
				_unity.Commit();
			}
			VoteItemDto voteDto = Mapper.Map<VoteItemDto>(voteItem);
			return Clients.Group(room).onVoteItemOpened(voteDto);
		}

		[SignalRAuth(Roles = "ScrumMaster")]
		public Task CloseVoteItem(string room, int voteItemId, int mark)
		{
			IRepository<VoteItem> repo = GetRepository<VoteItem>();
			VoteItem vote = repo.Get(x => x.Id == voteItemId).FirstOrDefault();
			if (vote != null)
			{
				vote.OverallMark = mark;
				vote.Closed = true;
				vote.Opened = false;
				//vote.VotedUsers.Clear();
				_unity.Commit();
			}
			VoteItemDto voteDto = Mapper.Map<VoteItemDto>(vote);
			return Clients.Group(room).onVoteItemClosed(voteDto);
		}

		private IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase
		{
			return _unity.GetRepository<TEntity>();
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}

		private Task EmptyTask
		{
			get
			{
				return Task.Factory.StartNew(() => { });
			}
		}

	}
}