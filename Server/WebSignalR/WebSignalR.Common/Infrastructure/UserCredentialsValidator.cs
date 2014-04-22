using System;
using System.Collections.Generic;
using System.Linq;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Infrastructure
{
	public sealed class UserCredentialsValidator : IEntityValidator, IDisposable
	{
		private IUnityOfWork _unity;
		private bool isValid;

		public UserCredentialsValidator(IUnityOfWork unity)
		{
			_unity = unity;
		}

		public bool IsValid<TEntity>(TEntity item) where TEntity : EntityBase
		{
			if (item == null)
				return false;

			IRepository<User> repo = _unity.GetRepository<User>();
			User usr = (item as User);
			if (usr == null)
				return false;

			isValid = (repo.Get(x => x.Name == usr.Name && x.Password == usr.Password).FirstOrDefault() != null);
			return isValid;
		}

		public IEnumerable<string> GetInvalidMessages<TEntity>(TEntity item) where TEntity : EntityBase
		{
			if (!isValid)
				yield return "User credentials validation was fail.";
			else
				yield break;
		}

		public void Dispose()
		{
			if (_unity != null)
				_unity.Dispose();
		}
	}
}