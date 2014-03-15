using System;
using System.Web;
using System.Web.Caching;

namespace WebSignalR.Infrastructure.Adapters
{
	public interface ICacheStorage
	{
		void Remove(string key);

		void Store(string key, object data);

		T Retrieve<T>(string key);
	}

	public class HttpContextCacheAdapter : ICacheStorage
	{
		public void Remove(string key)
		{
			if (HttpContext.Current != null)
				HttpContext.Current.Cache.Remove(key);
		}

		public void Store(string key, object data)
		{
			if (HttpContext.Current != null)
				HttpContext.Current.Cache.Insert(key, data, null, DateTime.UtcNow.AddMinutes(10), Cache.NoSlidingExpiration);
		}

		public T Retrieve<T>(string key)
		{
			if (HttpContext.Current == null)
				return default(T);
			T itemStored = (T)HttpContext.Current.Cache.Get(key);
			if (itemStored == null)
				itemStored = default(T);
			return itemStored;
		}
	}
}