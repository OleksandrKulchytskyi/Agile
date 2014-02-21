using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IContext : IDisposable
	{
		bool IsAuditEnabled { get; set; }

		DbTransaction BeginTransaction();
		int Commit();

		IDbSet<T> GetEntitySet<T>() where T : Common.Entities.EntityBase;
		void ChangeState<T>(T entity, EntityState state) where T : Entities.EntityBase;
		
		
	}
}