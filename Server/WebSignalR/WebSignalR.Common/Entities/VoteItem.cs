﻿using System.Collections.Generic;
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

		private bool isOpened;

		public bool Opened
		{
			get { return isOpened; }
			set { isOpened = value; OnPropChanged("Opened"); }
		}

		private bool isFinished;

		public bool Finished
		{
			get { return isFinished; }
			set { isFinished = value; OnPropChanged("Finished"); }
		}

		public int? RoomId { get; set; }

		public virtual Room HostRoom { get; set; }

		public virtual ICollection<UserVote> VotedUsers { get; set; }
	}
}