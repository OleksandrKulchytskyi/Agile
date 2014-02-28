using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Entities;

namespace WebSignalR.Common.Services
{
	public interface IUserRoomService
	{
		Room DisconnecFromRoomBySessionId(string room, string sessionId);
		Room JoinToRoomBySessionId(string roomName, string sessionId);

		void ChangeRoomState(string roomName, bool active);

		bool IsRoomActive(int roomId);

		void DisconnectAllUsers(string roomName);
	}
}