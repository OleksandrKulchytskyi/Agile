using System.Threading.Tasks;

namespace WebSignalR.Common.Services
{
	public interface IVoteService
	{
		Task AddVote(string roomName, string content);

		Task RemoveVote(string roomName, int voteId);

		Task VoteForItem(string room, int voteItemId, int mark);

		Task CloseVoteItem(string room, int voteItemId, int mark);

		Task OpenVoteItem(string room, int voteItemId);
	}
}