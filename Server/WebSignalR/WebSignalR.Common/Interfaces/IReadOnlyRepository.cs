using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IReadOnlyRepository<E> where E : Entities.EntityBase
	{
		IContext DbContext { get; set; }

		bool Contains(E id);
		E Get(E id);
		IQueryable<E> Get(Expression<Func<E, bool>> predicate);
		IEnumerable<E> GetAll();

		bool Exist(Expression<Func<E, bool>> predicate = null);
		int Count(Expression<Func<E, bool>> predicate = null);

		IPage<E> Page(int page = 1, int pageSize = 10);
	}

	public interface IRepository<E> : IReadOnlyRepository<E> where E : Entities.EntityBase
	{
		void Add(E entity);
		void Remove(E entity);
		void Update(E entity);
	}
}