using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace WebSignalR.Handlers
{
	/// <summary>
	/// Summary description for LoginHandler1
	/// </summary>
	public class LoginHandler : IHttpHandler
	{
		private const string Realm = "My Realm";
		private const string BasicAuthResponseHeaderValue = "Basic";
		private const string BasicAuthResponseHeader = "WWW-Authenticate";
		private const string BasicAuthHeader = "Authorization";
		private const string httpAuth = "HTTP_AUTH";


		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			if (context.Request.IsAuthenticated)
			{
			} 
			string authorization = context.Request.Headers["Authorization"];
			if (!string.IsNullOrEmpty(authorization) && authorization.IndexOf("Basic", StringComparison.OrdinalIgnoreCase) != -1)
			{

				bool result = AuthenticateUser(authorization.Replace("Basic ", "").Trim(), context);
				if (!result)
				{
					context.Response.Headers.Add(BasicAuthResponseHeader, "Basic realm=\"Test\"");
					context.Response.ContentType = "text/plain";
					context.Response.Write("You must authenticate");
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				}
				else
				{
					context.Response.ContentType = "text/plain";
					context.Response.StatusDescription = "Authorized.";
					context.Response.StatusCode = (int)HttpStatusCode.OK;
				}
			}
		}

		private bool AuthenticateUser(string credentials, HttpContext context)
		{
			bool validated = false;
			try
			{
				var encoding = Encoding.GetEncoding("UTF-8");
				credentials = encoding.GetString(Convert.FromBase64String(credentials));

				int separator = credentials.IndexOf(':');
				string name = credentials.Substring(0, separator);
				string password = credentials.Substring(separator + 1);

				validated = Login(context, name, password);
				if (validated)
				{
					WebSignalR.Infrastructure.CustomPrincipal principal = new Infrastructure.CustomPrincipal(name);
					SetPrincipal(principal);
					//var identity = new GenericIdentity(name);
					//SetPrincipal(new GenericPrincipal(identity, null));
				}
			}
			catch (FormatException)
			{
				// Credentials were not formatted correctly.
				validated = false;

			}
			return validated;
		}

		// TODO: validate the username and password.
		private bool CheckPassword(string username, string password)
		{
			return username == "user" && password == "password";
		}

		private bool Login(HttpContext context, string strUser, string strPwd)
		{
			if (CheckPassword(strUser, strPwd))
			{
				FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
				   1,                            // version
				   strUser,                      // user name
				   DateTime.Now,                 // create time
				   DateTime.Now.AddMinutes(30),  // expire time
				   false,                        // persistent
				   string.Empty);                          // user data

				string strEncryptedTicket = FormsAuthentication.Encrypt(ticket);
				HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strEncryptedTicket);
				context.Response.Cookies.Add(cookie);
				return true;
			}
			else
				return false;
		}

		private void SetPrincipal(IPrincipal principal)
		{
			Thread.CurrentPrincipal = principal;
			if (HttpContext.Current != null)
			{
				HttpContext.Current.User = principal;
			}
		}

		private void LogOut()
		{
			// Deprive client of the authentication key
			FormsAuthentication.SignOut();
		}
	}
}