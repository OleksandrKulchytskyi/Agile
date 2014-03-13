using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebSignalR.Common.Services
{
	public interface IVoteService
	{
		Task AddVote(string roomName, string content);
		Task RemoveVote(string roomName, int voteId);

		Task VoteForItem(string room, int voteItemId, int userId, int mark);
		Task CloseVoteItem(string room, int voteItemId, int mark);

	}
}