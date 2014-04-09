using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Services
{
	public class CsvStatePusher : ICsvStatePusher
	{
		private static readonly ConcurrentBag<StreamWriter> _streammessage;
		private int count = 0;

		static CsvStatePusher()
		{
			_streammessage = new ConcurrentBag<StreamWriter>();
		}

		public void OnStreamAvailable(Stream stream)
		{
			StreamWriter streamwriter = new StreamWriter(stream);
			_streammessage.Add(streamwriter);
			System.Threading.Interlocked.Increment(ref count);
		}

		public void Notify(Common.Interfaces.IBroadcastMessage msg)
		{
			List<StreamWriter> filtered = new List<StreamWriter>(System.Threading.Interlocked.CompareExchange(ref count, 0, 0));
			while (!_streammessage.IsEmpty)
			{
				StreamWriter sw;
				if (_streammessage.TryTake(out sw))
				{
					if (sw.BaseStream.CanWrite)
					{
						sw.WriteLine("data:" + JsonConvert.SerializeObject(msg) + "\n\n");
						sw.Flush();
						filtered.Add(sw);
					}
					else
					{
						System.Threading.Interlocked.Decrement(ref count);
					}
				}
			}

			foreach (var clinet in filtered)
			{
				_streammessage.Add(clinet);
			}
		}

	}
}