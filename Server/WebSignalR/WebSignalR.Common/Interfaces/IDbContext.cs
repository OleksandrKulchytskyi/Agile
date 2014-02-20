using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IDbContext
	{
		IDbSet<TEntity> Set<TEntity>() where TEntity : Entities.EntityBase;
		int SaveChanges();
		void Dispose();
	}

	public interface IContext : IDisposable
	{
		bool IsAuditEnabled { get; set; }
		IDbSet<T> GetEntitySet<T>() where T : class;
		void ChangeState<T>(T entity, EntityState state) where T : Entities.EntityBase;
		DbTransaction BeginTransaction();
		int Commit();
	}
}