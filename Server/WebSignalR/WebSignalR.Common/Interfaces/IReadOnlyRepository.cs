using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IReadOnlyRepository<E, T> where E : IEntity<T>
	{
		int Count { get; }

		bool Contains(T id);

		E Get(T id);

		IQueryable<E> Get(Expression<Func<E, bool>> predicate);

		IEnumerable<E> GetAll();
	}

	public interface IRepository<E, T> : IReadOnlyRepository<E, T> where E : IEntity<T>
	{
		void Add(E entity);

		void Remove(T id);

		void Update(E entity);
	}
}