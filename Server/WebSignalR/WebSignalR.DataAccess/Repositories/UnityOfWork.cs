using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.DataAccess.Repositories
{
	public class UnityOfWork : IUnityOfWork
	{
		private bool _disposed = false;
		private readonly IContext _context;

		private Dictionary<Type, object> _repositories = null;
		private DbTransaction _transaction = null;

		public UnityOfWork(IRepository<User> userRepo, IRepository<Room> roomRepo,
							IRepository<Privileges> privilegeRepo, IRepository<VoteItem> voteRepo,
							IRepository<UserSession> sessionRepo, IRepository<UserVote> userVoteRepo,
							ISessionRoomRepository srRepo, IContext context)
		{
			_context = context;
			_repositories = new Dictionary<Type, object>();

			AddRepository<Room>(roomRepo);
			AddRepository<User>(userRepo);
			AddRepository<UserSession>(sessionRepo);
			AddRepository<Privileges>(privilegeRepo);
			AddRepository<VoteItem>(voteRepo);
			AddRepository<UserVote>(userVoteRepo);
			AddRepository<SessionRoom>(srRepo);
		}

		public bool EnableAuditLog
		{
			get { return _context.IsAuditEnabled; }
			set { _context.IsAuditEnabled = value; }
		}

		public DbTransaction BeginTransaction()
		{
			_transaction = _context.BeginTransaction();
			return _transaction;
		}

		public void RollbackChanges()
		{
			if (_transaction != null)
				_transaction.Rollback();
		}

		public int Commit()
		{
			bool saveFailed = false;
			int rowsAffected = 0;
			const int retryCount = 2;
			System.Data.Entity.Infrastructure.DbUpdateConcurrencyException exInst = null;
			int retries = 0;
			do
			{
				retries++;
				try
				{
					rowsAffected = _context.Commit();
					saveFailed = false;
				}
				catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
				{
					exInst = ex;
					saveFailed = true;
					ex.Entries.ToList<System.Data.Entity.Infrastructure.DbEntityEntry>().ForEach(
						(entry) => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
				}
			} while (saveFailed && retries != retryCount);

			if (retries == 2)
				throw exInst;

			return rowsAffected;
		}

		public IRepository<TSet> GetRepository<TSet>() where TSet : Common.Entities.EntityBase
		{
			if (_repositories.Keys.Contains(typeof(TSet)))
				return _repositories[typeof(TSet)] as IRepository<TSet>;
			return null;
		}

		public void AddRepository<TSet>(object repository) where TSet : Common.Entities.EntityBase
		{
			if (!_repositories.ContainsKey(typeof(TSet)))
			{
				if (!object.ReferenceEquals((repository as IRepository<TSet>).DbContext, this._context))
					(repository as IRepository<TSet>).DbContext = _context;
				_repositories.Add(typeof(TSet), repository);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_repositories.Clear();

					if (null != _transaction && null != _transaction.Connection)
						_transaction.Connection.Dispose();

					if (null != _context && null != _context)
						_context.Dispose();
				}
				_disposed = true;
			}
		}
	}
}