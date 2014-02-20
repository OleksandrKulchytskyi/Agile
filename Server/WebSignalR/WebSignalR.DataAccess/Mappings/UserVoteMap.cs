﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class UserVoteMap : MapBase<UserVote>
	{
		public UserVoteMap()
			: base()
		{
			this.ToTable("UserVotes");

			this.Property(x => x.Mark).HasColumnName("Mark").IsRequired();
		}
	}
}
