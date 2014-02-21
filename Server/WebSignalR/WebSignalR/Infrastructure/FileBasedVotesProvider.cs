using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure
{
	public class FileBasedVotesProvider : IVotesProvider
	{
		public ICollection<Common.Entities.VoteItem> GetVotes()
		{
			//TODO: change here, data needs to be grabbed from a file.
			return new List<VoteItem>()
			{
				new VoteItem(){ Content="Question 1"},
				new VoteItem(){ Content="Question 2"},
				new VoteItem(){ Content="Question 3"},
				new VoteItem(){ Content="Question 4"}
			};
		}
	}
}