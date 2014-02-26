using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebSignalR.Common.Infrastructure;

namespace WebSignalR.Common.Entities
{
	public class VoteItem : EntityBase
	{
		public VoteItem()
		{
			VotedUsers = new SafeCollection<UserVote>();
		}

		private string content;
		[Required]
		public string Content
		{
			get { return content; }
			set { content = value; OnPropChanged("Content"); }
		}

		private int overallMark;
		public int OverallMark
		{
			get { return overallMark; }
			set { overallMark = value; OnPropChanged("OverallMark"); }
		}

		private bool closed;
		public bool Closed
		{
			get { return closed; }
			set { closed = value; OnPropChanged("Closed"); }
		}

		public int? RoomId { get; set; }
		public virtual Room HostRoom { get; set; }

		public virtual ICollection<UserVote> VotedUsers { get; set; }
	}
}