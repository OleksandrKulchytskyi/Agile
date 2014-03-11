using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.DTO
{
	public class RoomDto:DtoBase
	{
		public RoomDto()
		{
			ConnectedUsers = new List<UserDto>();
			ItemsToVote = new List<VoteItemDto>();
		}

		public bool Active { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		public List<UserDto> ConnectedUsers { get; set; }
		public List<VoteItemDto> ItemsToVote { get; set; }
	}
}