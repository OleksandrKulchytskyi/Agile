using System;
using System.ComponentModel.DataAnnotations;

namespace WebSignalR.Common.Entities
{
	public class SessionRoom : EntityBase
	{
		private string roomName;

		public string RoomName
		{
			get { return roomName; }
			set { roomName = value; OnPropChanged("RoomName"); }
		}

		public DateTimeOffset LoggedIn { get; set; }

		public UserSession Session { get; set; }

		public void SetLoggedInNow()
		{
			LoggedIn = DateTime.UtcNow;
		}
	}
}