using System;

namespace WebSignalR.Common.Interfaces
{
	public interface IPurge : IDisposable
	{
		void AddPurgeTask(Guid id);
	}
}