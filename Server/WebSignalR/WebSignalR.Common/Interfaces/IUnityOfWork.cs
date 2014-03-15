using System;
using System.Data.Common;

namespace WebSignalR.Common.Interfaces
{
	public interface IUnityOfWork : IRepositoryContainer, IDisposable
	{
		bool EnableAuditLog { get; set; }

		/// <summary>
		/// Initiate transaction
		/// </summary>
		/// <returns></returns>
		DbTransaction BeginTransaction();

		/// <summary>
		/// Commit all changes made in a container.
		/// </summary>
		///<remarks>
		/// If the entity have fixed properties and any optimistic concurrency problem exists,
		/// then an exception is thrown
		///</remarks>
		int Commit();

		/// <summary>
		/// Rollback tracked changes. See references of UnitOfWork pattern
		/// </summary>
		void RollbackChanges();
	}

	public interface IRepositoryContainer
	{
		IRepository<TSet> GetRepository<TSet>() where TSet : Entities.EntityBase;

		void AddRepository<TSet>(object repository) where TSet : Entities.EntityBase;
	}
}