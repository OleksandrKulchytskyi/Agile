using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Handlers
{
	public class DecompressionHandler : HttpClientHandler
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
			var responseTask = base.SendAsync(request, cancellationToken).ContinueWithExt<HttpResponseMessage, HttpResponseMessage>(prevtask =>
				{
					prevtask.Wait();

					var response = prevtask.Result;
					if (response.Content.Headers.ContentEncoding.Count > 0)
					{
						var encoding = response.Content.Headers.ContentEncoding.First();

						var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));

						if (compressor != null)
						{
							var decompression = DecompressContent(response.Content, compressor);
							decompression.Wait();
							response.Content = decompression.Result;
							return response;
						}
					}
					return response;
				});

			return responseTask;
		}

		private static Task<HttpContent> DecompressContent(HttpContent compressedContent, ICompressor compressor)
		{
			using (compressedContent)
			{
				MemoryStream decompressed = new MemoryStream();
				var readTask = compressedContent.ReadAsStreamAsync();
				readTask.Wait();
				return compressor.Decompress(readTask.Result, decompressed).ContinueWithExt<HttpContent>(prev =>
				{
					var newContent = new StreamContent(decompressed);// copy content type so we know how to load correct formatter
					newContent.Headers.ContentType = compressedContent.Headers.ContentType;
					return newContent;
				});
			}
		}
	}
}