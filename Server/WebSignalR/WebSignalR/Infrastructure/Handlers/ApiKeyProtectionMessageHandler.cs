using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WebSignalR.Common.Extension;

namespace WebSignalR.Infrastructure.Handlers
{
	public class ApiKeyProtectionMessageHandler : DelegatingHandler
	{
		protected async override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			IEnumerable<string> values;
			request.Headers.TryGetValues("apikey", out values);
			if (values != null && values.Count() == 1)
				return await base.SendAsync(request, cancellationToken);

			return await TaskHelper.FromResult<HttpResponseMessage>(new
				HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "Api key is required." });
		}
	}
}