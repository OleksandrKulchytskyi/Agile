using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;

namespace WebSignalR.Controllers
{
	[WebSignalR.Infrastructure.Filters.ValidateHttpAntiForgeryToken]
	public class RoomController : BaseController
	{
		private readonly IUnityOfWork _unity;
		private readonly IUserRoomService _userRoomService;

		public RoomController(IUnityOfWork unity, IUserRoomService userRoomService)
		{
			_unity = unity;
			_userRoomService = userRoomService;
		}

		[HttpGet]
		[ActionName("ByName")]
		public Room ByName([FromUri]string name)
		{
			try
			{
				IReadOnlyRepository<Room> repo = _unity.GetRepository<Room>();
				Room room = repo.Get(x => x.Name == name).FirstOrDefault();
				if (room != null)
					return room;
				else
					throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Room with such name cannot be found.") });

			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}
		}

		[HttpGet]
		[ActionName("GetRooms")]
		public IEnumerable<Room> GetRooms()
		{
			try
			{
				IReadOnlyRepository<Room> repo = _unity.GetRepository<Room>();
				IEnumerable<Room> rooms = repo.GetAll();
				return rooms;
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}
		}

		[HttpPost]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
		public HttpResponseMessage CreateRoom(RoomDto roomdto)
		{
			Room room = AutoMapper.Mapper.Map<Room>(roomdto);
			if (room == null || !ModelState.IsValid)
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

			// Need to detach to avoid loop reference exception during JSON serialization
			Room repoRoom = repo.Get(x => x.Name == room.Name).First();
			roomdto.Id = repoRoom.Id;
			this.TestHubContext.Clients.All.onRoomAdded(roomdto);

			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, roomdto);
			response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = roomdto.Id }));
			return response;
		}

		[HttpDelete]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
		public HttpResponseMessage DeleteRoom([FromUri]int id)
		{
			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room room;
			if ((room = repo.Get(x => x.Id == id).FirstOrDefault()) == null)
				return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("No room with such id.") };

			try
			{
				repo.Remove(room);
				_unity.Commit();
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
			// Need to detach to avoid loop reference exception during JSON serialization
			RoomDto dto = AutoMapper.Mapper.Map<RoomDto>(room);
			this.TestHubContext.Clients.All.onRoomDeleted(dto);
			return Request.CreateResponse(HttpStatusCode.OK, dto);
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
					RoomDto dto = AutoMapper.Mapper.Map<RoomDto>(room);
					TestHubContext.Clients.Group(room.Name).onJoinedRoom(dto);
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
		public bool IsRoomActive([FromUri]int roomId)
		{
			try
			{
				return _userRoomService.IsRoomActive(roomId);
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				throw ex;
			}
		}

		[HttpPut]
		public HttpResponseMessage LeaveRoom([FromUri]int roomId, [FromUri] int userId)
		{
			IRepository<User> repoUser = _unity.GetRepository<User>();
			IRepository<Room> repoRoom = _unity.GetRepository<Room>();

			try
			{
				User usr = repoUser.Get(x => x.Id == userId).FirstOrDefault();
				Room room = repoRoom.Get(x => x.Id == roomId).FirstOrDefault();
				if (usr != null && room != null)
				{
					room.ConnectedUsers.Remove(usr);
					_unity.Commit();
					RoomDto dto = AutoMapper.Mapper.Map<RoomDto>(room);
					TestHubContext.Clients.Group(room.Name).onLeftRoom(dto);
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