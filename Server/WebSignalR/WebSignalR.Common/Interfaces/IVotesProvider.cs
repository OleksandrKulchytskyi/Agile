using System.Collections.Generic;
using WebSignalR.Common.Entities;

namespace WebSignalR.Common.Interfaces
{
	public interface IVotesProvider
	{
		object Source { get; set; }

		ICollection<VoteItem> GetVotes();
	}
}