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
		protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
			challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
			throw new HttpResponseException(challengeMessage);
		}

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


	//public class AuthorizationFilterAttribute : ActionFilterAttribute
	//{
	//	private SecurityHeader _securityHeader;
	//	private SecurityCookieHeader _cookieHeader;
	//	public override void OnActionExecuting(HttpActionContext actionContext)
	//	{
	//		try
	//		{
	//			var controller = (AuthorizedController)actionContext.ControllerContext.Controller;
	//			var header =
	//				actionContext.Request.Headers.GetCookies(FormsAuthentication.FormsCookieName);
	//			if (header != null && header.Count > 0)
	//			{
	//				var cookie =
	//					header.First()
	//						.Cookies.FirstOrDefault(one => one.Name == FormsAuthentication.FormsCookieName);

	//				if (cookie != null && cookie.Value != null)
	//				{
	//					FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

	//					if (ticket != null)
	//					{
	//						_cookieHeader =
	//							JsonConvert.DeserializeObject<SecurityCookieHeader>(ticket.UserData);
	//						_securityHeader = controller.CacheProvider.Get<SecurityHeader>(_cookieHeader.Email);
	//						if (_securityHeader == null)
	//						{
	//							SetSecurityHeader();
	//						}
	//						if (!SecurityHelper.Login(_securityHeader).Identity.IsAuthenticated)
	//						{
	//							throw new SecurityException("Unable to login");
	//						}
	//						controller.SecurityHeader = _securityHeader;
	//						base.OnActionExecuting(actionContext);
	//					}
	//					else
	//					{
	//						throw new SecurityException("Unable to login");
	//					}
	//				}
	//				else
	//				{
	//					throw new SecurityException("Unable to login");
	//				}
	//			}
	//			else
	//			{
	//				throw new SecurityException("Unable to login");
	//			}

	//		}
	//		catch (SecurityException exception)
	//		{
	//			var controller = (AuthorizedController)actionContext.ControllerContext.Controller;
	//			controller.ExceptionHandler.HandleException(exception);
	//			actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
	//			{
	//				Content = new StringContent(JsonConvert.SerializeObject(
	//					new ExecutionResult<bool?>
	//					{
	//						ErrorMessage = "Invalid login.",
	//						Result = null,
	//						Success = false
	//					}))
	//			};
	//		}

	//	}

	//	private void SetSecurityHeader()
	//	{
	//		// cache the user data for speed and set it on a property of AuthorizedController to identify the user inside the controller action
	//	}
	//}
}