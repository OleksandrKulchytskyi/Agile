using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Entities;

namespace WebSignalR.Common.Interfaces
{
	public interface IVotesProvider
	{
		ICollection<VoteItem> GetVotes();
	}
}