using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Handlers
{
	public class DecompressionHandler : DelegatingHandler
	{
		public Collection<ICompressor> Compressors;

		public DecompressionHandler()
		{
			Compressors = new Collection<ICompressor>();
			Compressors.Add(new GZipCompressor());
			Compressors.Add(new DeflateCompressor());
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			var response = base.SendAsync(request, cancellationToken).Then<HttpResponseMessage, HttpResponseMessage>(responseMsg =>
			{
				if (responseMsg.Content.Headers.ContentEncoding.Count > 0 && responseMsg.Content != null)
				{
					var encoding = responseMsg.Content.Headers.ContentEncoding.First();

					var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));

					if (compressor != null)
					{
						var decompression = DecompressContentAsync(responseMsg.Content, compressor);
						decompression.Wait();
						responseMsg.Content = decompression.Result;
					}
				}

				return responseMsg;
			});

			return response;
		}

		private static Task<HttpContent> DecompressContentAsync(HttpContent compressedContent, ICompressor compressor)
		{
			using (compressedContent)
			{
				MemoryStream decompressed = new MemoryStream();
				var readTask = compressedContent.ReadAsStreamAsync();
				readTask.Wait();

				return compressor.Decompress(readTask.Result, decompressed).ContinueWithExt<HttpContent>(prevTask =>
				{
					// set position back to 0 so it can be read again
					decompressed.Position = 0;
					var newContent = new StreamContent(decompressed);
					// copy content type so we know how to load correct formatter
					newContent.Headers.ContentType = compressedContent.Headers.ContentType;
					return newContent;
				});
			}
		}
	}
}