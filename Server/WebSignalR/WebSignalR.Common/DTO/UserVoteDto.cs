using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.DTO
{
	public class UserVoteDto : DtoBase
	{
		public int Mark { get; set; }

		public int VoteItemId { get; set; }

		public int UserId { get; set; }

	}
}