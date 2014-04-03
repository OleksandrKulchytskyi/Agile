using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Http
{
	public class CompressedContent : HttpContent
	{
		private readonly HttpContent content;
		private readonly ICompressor compressor;

		public CompressedContent(HttpContent content, ICompressor compressor)
		{
			Ensure.Argument.NotNull(content, "content");
			Ensure.Argument.NotNull(compressor, "compressor");

			this.content = content;
			this.compressor = compressor;

			AddHeaders();
		}

		protected override bool TryComputeLength(out long length)
		{
			length = -1;
			return false;
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
		{
			Ensure.Argument.NotNull(stream, "stream");

			using (content)
			{
				var result = content.ReadAsStreamAsync();
				result.Wait();
				return compressor.Compress(result.Result, stream);
			}
		}

		private void AddHeaders()
		{
			foreach (var header in content.Headers)
			{
				Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			Headers.ContentEncoding.Add(compressor.EncodingType);
		}
	}
}