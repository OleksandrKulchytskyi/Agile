using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;

namespace WebSignalR.Infrastructure
{
	public sealed class UserRoomService : IUserRoomService, IDisposable
	{
		private enum Action
		{
			Add,
			Remove
		}

		private readonly IUnityOfWork _unity;

		public UserRoomService(IUnityOfWork unity)
		{
			_unity = unity;
		}

		public void DisconnectAllUsers(string roomName)
		{
			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room room = repo.Get(x => x.Name == roomName).FirstOrDefault();
			if (room != null)
			{
				room.ConnectedUsers.Clear();
				_unity.Commit();
			}
		}

		public Room DisconnecFromRoomBySessionId(string roomName, string sessionId)
		{
			return CommonMthd(roomName, sessionId, Action.Remove);
		}

		public Room JoinToRoomBySessionId(string roomName, string sessionId)
		{
			return CommonMthd(roomName, sessionId, Action.Add);
		}

		private Room CommonMthd(string roomName, string sessionId, Action action)
		{
			IRepository<Room> roomRepo = _unity.GetRepository<Room>();
			IRepository<UserSession> sessionRepo = _unity.GetRepository<UserSession>();

			User loggedUser = sessionRepo.Get(x => x.SessionId == sessionId).Select(x => x.User).FirstOrDefault();
			Room room = roomRepo.Get(x => x.Name == roomName).FirstOrDefault();
			if (room != null && loggedUser != null)
			{
				if (action == Action.Remove)
					room.ConnectedUsers.Remove(loggedUser);
				else if (action == Action.Add)
					room.ConnectedUsers.Add(loggedUser);
				_unity.Commit();
			}
			return room;
		}

		public void ChangeRoomState(string roomName, bool active)
		{
			IRepository<Room> repo = _unity.GetRepository<Room>();
			Room room = repo.Get(x => x.Name == roomName).FirstOrDefault();
			if (room != null)
			{
				room.Active = active;
				_unity.Commit();
			}
		}

		public void Dispose()
		{
			if (_unity != null)
				_unity.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}