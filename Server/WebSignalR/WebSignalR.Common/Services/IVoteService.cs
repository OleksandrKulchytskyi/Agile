using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebSignalR.Common.Services
{
	public interface IVoteService
	{
		Task VoteForItem(string room, int voteItemId, int userId, int mark);
		Task CloseVoteItem(string room, int voteItemId, int mark);

	}
}