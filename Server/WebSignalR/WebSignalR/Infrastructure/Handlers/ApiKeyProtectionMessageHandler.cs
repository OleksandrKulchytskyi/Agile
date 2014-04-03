using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using WebSignalR.Common.Extension;

namespace WebSignalR.Infrastructure.Handlers
{
	public class ApiKeyProtectionMessageHandler : DelegatingHandler
	{
		protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			IEnumerable<string> values;
			request.Headers.TryGetValues("apikey", out values);
			if (values != null && values.Count() == 1)
				return base.SendAsync(request, cancellationToken);

			return TaskHelper.FromResult<HttpResponseMessage>(new
				HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "Api key is required." });
		}
	}
}