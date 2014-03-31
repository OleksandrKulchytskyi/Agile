
namespace WebSignalR.Common.Interfaces
{
	public interface IPagedList
	{
		int TotalCount { get; set; }
		int PageCount { get; set; }
		int PageIndex { get; set; }
		int PageSize { get; set; }
	}
}