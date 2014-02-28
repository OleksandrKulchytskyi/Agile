using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;

namespace WebSignalR.Controllers
{
	public class RoomController : BaseController
	{
		private readonly IUnityOfWork _unity;
		private readonly IUserRoomService _userRoomService;
		public RoomController(IUnityOfWork unity , IUserRoomService userRoomService)
		{
			_unity = unity;
			_userRoomService = userRoomService;
		}

		[HttpGet]
		public IEnumerable<Room> GetRooms()
		{
			try
			{
				IReadOnlyRepository<Room> repo = _unity.GetRepository<Room>();
				IEnumerable<Room>  rooms=repo.GetAll();
				return rooms;
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}
		}

		[Authorize(Roles="Admin")]
		[HttpPost]
		public HttpResponseMessage AddRoom(Room room)
		{
			if (room == null)
				return new HttpResponseMessage(HttpStatusCode.BadRequest);

			IRepository<Room> repo = _unity.GetRepository<Room>();
			if (repo.Exist(x => x.Name == room.Name))
				return new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("User with such name already exists.") };

			try
			{
				repo.Add(room);
				_unity.Commit();
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			Room repoRoom = repo.Get(x => x.Name == room.Name).First();
			this.TestHubContext.Clients.All.onRoomAdded(repoRoom);
			HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.Created);
			msg.Headers.Add("Id", repoRoom.Id.ToString());
			return msg;
		}

		[HttpGet]
		public HttpResponseMessage EnterRoom([FromUri]int roomId, [FromUri] int userId)
		{
			IRepository<User> repoUser = _unity.GetRepository<User>();
			IRepository<Room> repoRoom = _unity.GetRepository<Room>();

			try
			{
				User usr = repoUser.Get(x => x.Id == userId).FirstOrDefault();
				Room room = repoRoom.Get(x => x.Id == roomId).FirstOrDefault();
				if (usr != null && room != null)
				{
					room.ConnectedUsers.Add(usr);
					_unity.Commit();
					TestHubContext.Clients.Group(room.Name).onUserEntered(usr);
				}
				else
					return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Unable to found entities with such id's.") };

			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpGet]
		public HttpResponseMessage LeaveRoom([FromUri]int roomName, [FromUri] int userId)
		{
			IRepository<User> repoUser = _unity.GetRepository<User>();
			IRepository<Room> repoRoom = _unity.GetRepository<Room>();

			try
			{
				User usr = repoUser.Get(x => x.Id == userId).FirstOrDefault();
				Room room = repoRoom.Get(x => x.Id == roomName).FirstOrDefault();
				if (usr != null && room != null)
				{
					room.ConnectedUsers.Remove(usr);
					_unity.Commit();
					TestHubContext.Clients.Group(room.Name).onUserLeft(usr);
				}
				else
					return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Unable to found entities with such id's.") };

			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpPost]
		public HttpResponseMessage DisconnectFromRoom([FromUri]string roomName, [FromUri] string sessionId)
		{
			if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(sessionId))
				return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Unable to found entities with such id's.") };

			try
			{
				_userRoomService.DisconnecFromRoomBySessionId(roomName, sessionId);
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
			{
				_unity.Dispose();
			}
			base.Dispose(disposing);
		}

	}
}