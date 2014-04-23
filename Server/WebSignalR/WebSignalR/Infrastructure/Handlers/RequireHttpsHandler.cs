using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebSignalR.Common.Extension;

namespace WebSignalR.Infrastructure.Handlers
{
	/// <summary>
	/// A message handler that enforces all requests are made over HTTPS.
	/// </summary>
	public class RequireHttpsHandler : DelegatingHandler
	{
		private const string ReasonPhrase = "SSL Required.";

		protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
			{
				return await TaskHelper.FromMethod<HttpResponseMessage>(() =>
				{
					var response = request.CreateResponse(HttpStatusCode.Forbidden);
					response.ReasonPhrase = ReasonPhrase;
					return response;
				});
			}

			return await base.SendAsync(request, cancellationToken);
		}
	}
}