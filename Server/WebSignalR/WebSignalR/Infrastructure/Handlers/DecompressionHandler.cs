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

		protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken);

			if (response.Content.Headers.ContentEncoding.Count > 0 && response.Content != null)
			{
				var encoding = response.Content.Headers.ContentEncoding.First();
				var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));

				if (compressor != null)
				{
					var decompression = DecompressContentAsync(response.Content, compressor);
					response.Content = await decompression;
				}
			}

			return response;
		}

		private async static Task<HttpContent> DecompressContentAsync(HttpContent compressedContent, ICompressor compressor)
		{
			using (compressedContent)
			{
				MemoryStream decompressed = new MemoryStream();
				var readTask = compressedContent.ReadAsStreamAsync();

				await compressor.Decompress(await readTask, decompressed);
				// set position back to 0 so it can be read again
				decompressed.Position = 0;
				var newContent = new StreamContent(decompressed);
				// copy content type so we know how to load correct formatter
				newContent.Headers.ContentType = compressedContent.Headers.ContentType;
				return newContent;
			}
		}
	}
}