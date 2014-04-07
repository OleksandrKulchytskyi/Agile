using System.Collections.Concurrent;
using System.IO;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Services
{
	public class CsvStatePusher : ICsvStatePusher
	{
		private static readonly ConcurrentQueue<StreamWriter> _streammessage;

		static CsvStatePusher()
		{
			_streammessage = new ConcurrentQueue<StreamWriter>();
		}

		public void OnStreamAvailable(Stream stream)
		{
			StreamWriter streamwriter = new StreamWriter(stream);
			_streammessage.Enqueue(streamwriter);
		}

		public void Notify(Common.Interfaces.IBroadcastMessage msg)
		{
			foreach (var subscriber in _streammessage)
			{
				subscriber.WriteLine("data:" + Newtonsoft.Json.JsonConvert.SerializeObject(msg) + "\n");
				subscriber.Flush();
			}
		}
	}
}