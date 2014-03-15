using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace WebSignalR.Common.Interfaces
{
	/// <summary>
	/// Generic base interface for all services.
	/// Purpose:
	/// - Implement BusinessLogic in services
	/// - Hide IRepository from being exposed further out than service layer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TA"></typeparam>
	public interface IService<T, out TA>
	{
		IQueryable<T> GetAll();

		IQueryable<T> GetAllReadOnly();

		T GetById(int id);

		IEntityValidator SaveOrUpdate(T entity);

		void Delete(T entity);

		IEnumerable<T> Find(Expression<Func<T, bool>> expression, int maxHits = 100);

		IPage<T> Page(int page = 1, int pageSize = 10);
	}
}