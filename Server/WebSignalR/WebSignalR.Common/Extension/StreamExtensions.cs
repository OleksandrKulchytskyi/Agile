using System;
using System.IO;
using System.Threading.Tasks;

namespace WebSignalR.Common.Extension
{
	public static class StreamExtensions
	{
		public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize)
		{
			return Task.Factory.StartNew(() =>
			{
				if (source == null)
					throw new ArgumentNullException("source");

				if (destination == null)
					throw new ArgumentNullException("destination");

				if (bufferSize <= 0)
					throw new ArgumentOutOfRangeException("bufferSize", "bufferSize must be greater than zero");

				var bytesRead = 0;
				var buffer = new byte[bufferSize];

				while ((bytesRead = Task<int>.Factory.FromAsync(source.BeginRead, source.EndRead, buffer, 0, bufferSize, null).Result) > 0)
				{
					Task.Factory.FromAsync(destination.BeginWrite, destination.EndWrite, buffer, 0, bytesRead, null).Wait();
				}
			});
		}
	}
}