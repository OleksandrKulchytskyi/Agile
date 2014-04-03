using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using WebSignalR.Common.Extension;

namespace WebSignalR.Infrastructure.Handlers
{
	public class EmptyPostBodyMessageHandler : DelegatingHandler
	{
		protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			if (request.Content != null)
			{
				var content = request.Content.ReadAsStringAsync().Result;
				if (HttpMethod.Post == request.Method && content.Length == 0)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("Empty body has been detected in the POST request.");
#endif
					Global.Logger.Warn("Empty body has been detected in the POST request.");
					return TaskHelper.FromResult<HttpResponseMessage>(new 
							HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { ReasonPhrase = "Empty body not allowed for the POST request." });
				}
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}