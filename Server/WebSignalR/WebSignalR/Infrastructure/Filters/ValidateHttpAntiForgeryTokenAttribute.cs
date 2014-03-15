using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Validation;
using System.Web.Mvc;

namespace WebSignalR.Infrastructure.Filters
{
	public class ValidateHttpAntiForgeryTokenAttribute : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			HttpRequestMessage request = actionContext.ControllerContext.Request;
			object httpContext;
			if (request.Properties.TryGetValue("MS_HttpContext", out httpContext))
			{
				HttpContextBase httpCtxtBase = httpContext as HttpContextBase;
				if (httpCtxtBase.Request.IsLocal)
					return;
			}
			else if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put || request.Method == HttpMethod.Delete)
			{
				try
				{
					if (IsAjaxRequest(request))
						ValidateRequestHeader(request);
					else
						AntiForgery.Validate();
				}
				catch (HttpAntiForgeryException e)
				{
					actionContext.Response = request.CreateErrorResponse(HttpStatusCode.Forbidden, e);
				}
			}
		}

		private bool IsAjaxRequest(HttpRequestMessage request)
		{
			IEnumerable<string> xRequestedWithHeaders;
			if (request.Headers.TryGetValues("X-Requested-With", out xRequestedWithHeaders))
			{
				string headerValue = xRequestedWithHeaders.FirstOrDefault();
				if (!String.IsNullOrEmpty(headerValue))
				{
					return String.Equals(headerValue, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
				}
			}

			return false;
		}

		private void ValidateRequestHeader(HttpRequestMessage request)
		{
			string cookieToken = String.Empty;
			string formToken = String.Empty;

			IEnumerable<string> tokenHeaders;
			if (request.Headers.TryGetValues("RequestVerificationToken", out tokenHeaders))
			{
				string tokenValue = tokenHeaders.FirstOrDefault();
				if (!String.IsNullOrEmpty(tokenValue))
				{
					string[] tokens = tokenValue.Split(':');
					if (tokens.Length == 2)
					{
						cookieToken = tokens[0].Trim();
						formToken = tokens[1].Trim();
					}
				}
			}

			AntiForgery.Validate(cookieToken, formToken);
		}
	}
}