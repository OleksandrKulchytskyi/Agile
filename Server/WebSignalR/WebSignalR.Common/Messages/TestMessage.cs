using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Messages
{
	public class TestMessage:IBroadcastMessage
	{
		public string Data { get; set; }
	}
}