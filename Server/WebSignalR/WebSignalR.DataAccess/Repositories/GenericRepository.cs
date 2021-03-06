﻿using System;
using System.Collections.Generic;
using System.Linq;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.DataAccess.Repositories
{
	public class GenericRepository<T> : IRepository<T> where T : Common.Entities.EntityBase
	{
		public GenericRepository(IContext context)
		{
			DbContext = context;
		}

		public IContext DbContext
		{
			get;
			set;
		}

		private System.Data.Entity.IDbSet<TEntity> GetSet<TEntity>() where TEntity : Common.Entities.EntityBase
		{
			return this.DbContext.GetEntitySet<TEntity>();
		}

		public void Add(T entity)
		{
			GetSet<T>().Add(entity);
		}

		public bool Contains(T entity)
		{
			var set = GetSet<T>();
			return (entity == null) ? set.Any() : set.Any(x => x.Id == entity.Id);
		}

		public bool Exist(System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
		{
			var set = GetSet<T>();
			return (predicate == null) ? set.Any() : set.Any(predicate);
		}

		public void Update(T entity)
		{
			this.DbContext.ChangeState(entity, System.Data.Entity.EntityState.Modified);
		}

		public void Remove(T entity)
		{
			this.DbContext.ChangeState(entity, System.Data.Entity.EntityState.Deleted);
		}

		public void AddBulk(IEnumerable<T> items)
		{
			foreach (var item in items)
				GetSet<T>().Add(item);
		}

		public int Count(System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
		{
			var set = GetSet<T>();
			return (predicate == null) ?
				   set.Count() :
				   set.Count(predicate);
		}

		public T Get(T entity)
		{
			return GetSet<T>().FirstOrDefault(x => x.Id == entity.Id);
		}

		public IQueryable<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			return GetSet<T>().Where(predicate);
		}

		public IEnumerable<T> GetAll()
		{
			return GetSet<T>().AsEnumerable();
		}

		/// <summary>
		/// Provides paging possibilites
		/// </summary>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public IPage<T> Page(int page = 1, int pageSize = 10)
		{
			var internalPage = page - 1;
			var data = this.GetSet<T>().OrderBy(k => k.Id).Skip(pageSize * internalPage).Take(pageSize).AsEnumerable();
			return new Page<T>(data, this.GetSet<T>().Count(), pageSize, page);
		}
	}
}