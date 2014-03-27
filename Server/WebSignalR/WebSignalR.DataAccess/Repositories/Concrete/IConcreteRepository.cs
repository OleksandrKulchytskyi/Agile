using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.DataAccess.Repositories
{
	public interface IRoomRepository : IRepository<Room>
	{
	}

	public interface IUserRepository : IRepository<User>
	{
	}

	public interface IPrivilegeRepository : IRepository<Privileges>
	{
	}

	public interface ISessionRepository : IRepository<UserSession>
	{
	}

	public interface ISessionRoomRepository : IRepository<SessionRoom>
	{
	}

	public interface IUserVoteRepository : IRepository<UserVote>
	{
	}

	public interface IVoteItemRepository : IRepository<VoteItem>
	{
	}
}
