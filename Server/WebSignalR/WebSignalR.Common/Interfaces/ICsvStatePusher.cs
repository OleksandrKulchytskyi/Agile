using System.IO;

namespace WebSignalR.Common.Interfaces
{
	public interface ICsvStatePusher
	{
		void OnStreamAvailable(Stream stream);

		void Notify(IBroadcastMessage msg);
	}
}