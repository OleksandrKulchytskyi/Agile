using System.Collections.Generic;

namespace WebSignalR.Common.DTO
{
	public class VoteItemDto : DtoBase
	{
		public VoteItemDto()
		{
			VotedUsers = new List<int>();
		}

		public string Content { get; set; }

		public int OverallMark { get; set; }

		public bool Closed { get; set; }

		public bool Opened { get; set; }

		public bool Finished { get; set; }

		//public RoomDto HostRoom { get; set; }

		public int HostRoomId { get; set; }

		public List<int> VotedUsers { get; set; }
	}
}