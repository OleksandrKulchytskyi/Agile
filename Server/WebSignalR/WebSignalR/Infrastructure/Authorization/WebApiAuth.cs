using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;
using System.Web.Security;

namespace WebSignalR.Infrastructure.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class WebApiAuth : AuthorizeAttribute
	{
		[System.Diagnostics.DebuggerStepThrough]
		protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
			challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
			throw new HttpResponseException(challengeMessage);
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			try
			{
				IHttpController controller = actionContext.ControllerContext.Controller;
				var header = actionContext.Request.Headers.GetCookies(Infrastructure.Constants.FormsAuthKey);
				if (header != null && header.Count > 0)
				{
					System.Net.Http.Headers.CookieState cookie = header.First().Cookies.
								FirstOrDefault(one => one.Name == Infrastructure.Constants.FormsAuthKey);

					if (cookie != null && cookie.Value != null)
					{
						FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
						if (ticket != null)
						{
							CustomIdentity ci = new CustomIdentity(ticket); // Create a CustomIdentity based on the FormsAuthenticationTicket  
							CustomPrincipal principal = new CustomPrincipal(ci); // Create the CustomPrincipal
							if (this.Roles != null)
							{
								bool notFound = false;
								string[] roles = Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
#if DEBUG
								System.Diagnostics.Debug.WriteLine("Filter roles: " + Roles);
								System.Diagnostics.Debug.WriteLine("User roles: " + string.Join(",", principal.Roles));
#endif
								foreach (var item in roles)
								{
									if (!principal.IsInRole(item))
									{
										notFound = true; break;
									}
								}
								if (notFound)
									HandleUnauthorizedRequest(actionContext);
							}
							SetPrincipal(principal);
						}
					}
					else
						HandleUnauthorizedRequest(actionContext);
				}
			}
			catch (Exception)
			{
				var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
				challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
				throw new HttpResponseException(challengeMessage);
			}
		}

		private void SetPrincipal(System.Security.Principal.IPrincipal principal)
		{
			System.Threading.Thread.CurrentPrincipal = principal;
			if (System.Web.HttpContext.Current != null)
			{
				System.Web.HttpContext.Current.User = principal;
			}
		}
	}
}