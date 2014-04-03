using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;
using WebSignalR.Infrastructure.Http;

namespace WebSignalR.Infrastructure.Handlers
{
	public class CompressionHandler : DelegatingHandler
	{
		public Collection<ICompressor> Compressors { get; private set; }

		public CompressionHandler()
		{
			Compressors = new Collection<ICompressor>();

			Compressors.Add(new GZipCompressor());
			Compressors.Add(new DeflateCompressor());
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var responseTask = base.SendAsync(request, cancellationToken).Then<HttpResponseMessage, HttpResponseMessage>(respMsg =>
			{
				if (request.Headers.AcceptEncoding.Count > 0 && respMsg.Content != null)
				{
					// As per RFC2616.14.3:
					// Ignores encodings with quality == 0
					// If multiple content-codings are acceptable, then the acceptable content-coding with the highest non-zero qvalue is preferred.
					var compressor = (from encoding in request.Headers.AcceptEncoding
									  let quality = encoding.Quality ?? 1.0
									  where quality > 0
									  join c in Compressors on encoding.Value.ToLowerInvariant() equals c.EncodingType.ToLowerInvariant()
									  orderby quality descending
									  select c).FirstOrDefault();

					if (compressor != null)
					{
						respMsg.Content = new CompressedContent(respMsg.Content, compressor);
					}
				}

				return respMsg;
			});

			return responseTask;
		}
	}
}