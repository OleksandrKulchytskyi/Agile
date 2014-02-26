using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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