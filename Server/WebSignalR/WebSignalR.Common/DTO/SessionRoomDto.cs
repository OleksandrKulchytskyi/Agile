using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.DTO
{
	public class SessionRoomDto : DtoBase
	{
		public string RoomName { get; set; }
		public DateTimeOffset LoggedIn { get; set; }
	}
}