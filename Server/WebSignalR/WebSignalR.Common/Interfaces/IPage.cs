using System.Collections.Generic;

namespace WebSignalR.Common.Interfaces
{
	public interface IPage<T>
	{
		int CurrentPage { get; set; }

		int PagesCount { get; set; }

		int PageSize { get; set; }

		int Count { get; set; }

		IEnumerable<T> Entities { get; set; }
	}
}