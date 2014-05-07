using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Extension;
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
			Ensure.Argument.NotNull(unity, "unity");
			Ensure.Argument.NotNull(userRoomService, "userRoomService");
			_unity = unity;
			_userRoomService = userRoomService;
		}

		[HttpGet]
		[ActionName("InvokeTest")]
		public HttpResponseMessage InvokeTest(string data, string room)
		{
			try
			{
				AgileHubConnection.All.onTestMethod(data);
				AgileHubConnection.Group(room).onTestMethod(data + " : " + room);
			}
			catch (Exception ex)
			{
				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
			}
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpGet]
		[ActionName("ByName")]
		public async Task<Room> ByName([FromUri]string name)
		{
			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					IReadOnlyRepository<Room> repo = _unity.GetRepository<Room>();
					Room room = repo.Get(x => x.Name == name).FirstOrDefault();
					if (room != null)
						return room;
					else
						throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Room with such name cannot be found.") });
				});
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}
		}

		[HttpGet]
		[ActionName("GetRooms")]
		public async Task<IEnumerable<Room>> GetRooms()
		{
			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					IReadOnlyRepository<Room> repo = _unity.GetRepository<Room>();
					IEnumerable<Room> rooms = repo.GetAll();
					return rooms;
				});
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}
		}

		[HttpPost]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
		public async Task<HttpResponseMessage> CreateRoom(RoomDto roomdto)
		{
			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					Room room = AutoMapper.Mapper.Map<Room>(roomdto);
					if (room == null || !ModelState.IsValid)
						return new HttpResponseMessage(HttpStatusCode.BadRequest);

					IRepository<Room> repo = _unity.GetRepository<Room>();
					if (repo.Exist(x => x.Name == room.Name))
						return new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("Room with such name already exists.") };


					repo.Add(room);
					_unity.Commit();

					// Need to detach to avoid loop reference exception during JSON serialization
					Room repoRoom = repo.Get(x => x.Name == room.Name).FirstOrDefault();
					roomdto.Id = repoRoom.Id;
					base.AgileHubConnection.All.onRoomAdded(roomdto);

					HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, roomdto);
					response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = roomdto.Id }));
					return response;
				});
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		[HttpPut]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
		public HttpResponseMessage UpdateRoom(int id, RoomDto roomdto)
		{
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			if (id != roomdto.Id)
				return Request.CreateResponse(HttpStatusCode.BadRequest);

			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room repoRoom = repo.Get(x => x.Id == id).FirstOrDefault();
			try
			{
				repoRoom.Name = roomdto.Name;
				repoRoom.Description = roomdto.Description;
				repoRoom.Active = roomdto.Active;
				repo.Update(repoRoom);
				_unity.Commit();
			}
			catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError);
			}

			HttpResponseMessage msg = Request.CreateResponse(HttpStatusCode.OK);
			return msg;
		}

		[HttpDelete]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
		public async Task<HttpResponseMessage> DeleteRoom([FromUri]int id)
		{
			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					IRepository<Room> repo = _unity.GetRepository<Room>();
					Room room;
					if ((room = repo.Get(x => x.Id == id).FirstOrDefault()) == null)
						return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("No room with such id.") };

					repo.Remove(room);
					_unity.Commit();

					// Need to detach to avoid loop reference exception during JSON serialization
					RoomDto dto = AutoMapper.Mapper.Map<RoomDto>(room);
					this.AgileHubConnection.All.onRoomDeleted(dto);
					return Request.CreateResponse(HttpStatusCode.OK, dto);
				});
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		[HttpGet]
		public async Task<HttpResponseMessage> EnterRoom([FromUri]int roomId, [FromUri] int userId)
		{
			IRepository<User> repoUser = _unity.GetRepository<User>();
			IRepository<Room> repoRoom = _unity.GetRepository<Room>();

			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					User usr = repoUser.Get(x => x.Id == userId).FirstOrDefault();
					Room room = repoRoom.Get(x => x.Id == roomId).FirstOrDefault();
					if (usr != null && room != null)
					{
						room.ConnectedUsers.Add(usr);
						_unity.Commit();
						UserDto userDto = AutoMapper.Mapper.Map<UserDto>(usr);
						base.AgileHubConnection.Group(room.Name).onJoinedRoom(userDto);
					}
					else
						return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Unable to found entities with such id's.") };

					return Request.CreateResponse(HttpStatusCode.OK);
				});
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		[HttpGet]
		public async Task<bool> IsRoomActive([FromUri]int roomId)
		{
			try
			{
				return await TaskHelper.FromMethod(() => _userRoomService.IsRoomActive(roomId));
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				throw ex;
			}
		}

		[HttpPost]
		public async Task<HttpResponseMessage> LeaveRoom([FromUri]int roomId, [FromUri] int userId)
		{
			IRepository<User> repoUser = _unity.GetRepository<User>();
			IRepository<Room> repoRoom = _unity.GetRepository<Room>();
			IRepository<SessionRoom> srRepo = _unity.GetRepository<SessionRoom>();

			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					User usr = repoUser.Get(x => x.Id == userId).FirstOrDefault();
					Room room = repoRoom.Get(x => x.Id == roomId).FirstOrDefault();
					if (usr != null && room != null)
					{
						room.ConnectedUsers.Remove(usr);
						SessionRoom sr = srRepo.Get(x => x.Session.User.Id == userId).FirstOrDefault();
						if (sr != null)
							srRepo.Remove(sr);

						_unity.Commit();
						UserDto userDto = AutoMapper.Mapper.Map<UserDto>(usr);
						base.AgileHubConnection.Group(room.Name).onLeftRoom(userDto);
					}
					else
						return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Unable to found entities with such id's.") };

					return Request.CreateResponse(HttpStatusCode.OK);
				});
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		[HttpPost]
		public async Task<HttpResponseMessage> DisconnectFromRoom([FromUri]string roomName, [FromUri] string sessionId)
		{
			if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(sessionId))
				return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Unable to found entities with such id's.") };

			try
			{
				return await TaskHelper.FromMethod(() =>
				{
					_userRoomService.DisconnecFromRoomBySessionId(roomName, sessionId);

					IRepository<UserSession> sessionRepo = _unity.GetRepository<UserSession>();
					UserSession session = sessionRepo.Get(x => x.SessionId == sessionId).FirstOrDefault();
					if (session != null)
						AgileHubConnection.Group(roomName).onLeftRoom(AutoMapper.Mapper.Map<UserDto>(session.User));

					return new HttpResponseMessage(HttpStatusCode.OK);
				});
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
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