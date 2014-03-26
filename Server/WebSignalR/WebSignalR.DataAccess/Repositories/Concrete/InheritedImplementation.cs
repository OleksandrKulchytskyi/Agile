using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.DataAccess.Repositories
{
	public sealed class RoomRepository : GenericRepository<Room>, IRoomRepository
	{
		public RoomRepository(IContext ctx)
			: base(ctx)
		{ }
	}

	public sealed class UserRepository : GenericRepository<User>, IUserRepository
	{
		public UserRepository(IContext ctx)
			: base(ctx)
		{ }
	}

	public sealed class SessionRepository : GenericRepository<UserSession>, ISessionRepository
	{
		public SessionRepository(IContext ctx)
			: base(ctx)
		{ }
	}

	public sealed class PrvilegeRepository : GenericRepository<Privileges>, IPrivilegeRepository
	{
		public PrvilegeRepository(IContext ctx)
			: base(ctx)
		{ }
	}

	public sealed class VoteItemRepository : GenericRepository<VoteItem>, IVoteItemRepository
	{
		public VoteItemRepository(IContext ctx)
			: base(ctx)
		{ }
	}

	public sealed class UserVoteRepository : GenericRepository<UserVote>, IUserVoteRepository
	{
		public UserVoteRepository(IContext ctx)
			: base(ctx)
		{ }
	}
}
