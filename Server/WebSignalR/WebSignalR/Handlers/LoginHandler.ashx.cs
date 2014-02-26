using Ninject;
using System;
using System.Net;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Handlers
{
	/// <summary>
	/// Summary description for LoginHandler1
	/// </summary>
	public class LoginHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
	{
		private const string Realm = "My Realm";
		private const string BasicAuthResponseHeaderValue = "Basic";
		private const string BasicAuthResponseHeader = "WWW-Authenticate";
		private const string BasicAuthHeader = "Authorization";
		private const string httpContentType = "text/plain";

		public LoginHandler()
		{
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			string authorization = context.Request.Headers[BasicAuthHeader];
			if (!string.IsNullOrEmpty(authorization) && authorization.IndexOf(BasicAuthResponseHeaderValue, StringComparison.OrdinalIgnoreCase) != -1)
			{

				bool result = AuthenticateUser(authorization.Replace(BasicAuthResponseHeaderValue, string.Empty).Trim(), context);
				if (!result)
				{
					context.Response.Headers.Add(BasicAuthResponseHeader, "Basic realm=\"Test\"");
					context.Response.ContentType = httpContentType;
					context.Response.Write("You must authenticate");
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				}
				else
				{
					context.Response.ContentType = httpContentType;
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
					Infrastructure.CustomPrincipal principal = new Infrastructure.CustomPrincipal(name, true);
					SetPrincipal(principal);
				}
			}
			catch (FormatException ex)
			{
				Global.Logger.Error(ex);
				validated = false;
			}
			return validated;
		}

		private bool CheckPassword(string username, string password)
		{
			//return username == "user" && password == "password";
			IEntityValidator validator = Infrastructure.BootStrapper.Kernel.Get<IEntityValidator>("Credentials");
			if (validator != null)
			{
				return validator.IsValid<User>(new User() { Name = username, Password = password });
			}
			return false;
		}

		private bool Login(HttpContext context, string strUser, string strPwd)
		{
			if (CheckPassword(strUser, strPwd))
			{
				FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, strUser, DateTime.Now, DateTime.Now.AddMinutes(30), /*expire time*/ false,/*persistent*/ string.Empty /*user data*/);

				string strEncryptedTicket = FormsAuthentication.Encrypt(ticket);
				HttpCookie cookie = new HttpCookie(Infrastructure.Constants.FormsAuthKey, strEncryptedTicket);
				context.Response.Cookies.Add(cookie);
				return true;
			}
			else
				return false;
		}

		private void SetPrincipal(Infrastructure.CustomPrincipal principal)
		{
			Thread.CurrentPrincipal = principal;
			IUnityOfWork unity = Infrastructure.BootStrapper.Kernel.Get<IUnityOfWork>();
			using (unity)
			{
				IReadOnlyRepository<User> repo = unity.GetRepository<User>();
				User usr = repo.Get(x => x.Name == principal.Identity.Name).FirstOrDefault();
				if (usr != null)
				{
					principal.UserId = usr.Id;
					principal.Roles = usr.UserPrivileges.Select(x => x.Name).ToList();
				}
			}
			if (HttpContext.Current != null)
				HttpContext.Current.User = principal;
		}

		private void LogOut()
		{
			// Deprive client of the authentication key
			FormsAuthentication.SignOut();
		}
	}
}