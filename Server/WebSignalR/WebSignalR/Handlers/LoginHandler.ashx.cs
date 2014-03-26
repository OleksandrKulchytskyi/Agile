using Ninject;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Infrastructure;

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
		private const string PersistantHeader = "Persistant";
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
			bool isPersistent = false;
			string authorization = context.Request.Headers[BasicAuthHeader];
			if (context.Request.Headers.AllKeys.Contains(PersistantHeader))
			{
				if (context.Request.Headers[PersistantHeader] == "1")
					isPersistent = true;
			}
			if (!string.IsNullOrEmpty(authorization) && authorization.IndexOf(BasicAuthResponseHeaderValue, StringComparison.OrdinalIgnoreCase) != -1)
			{
				bool result = AuthenticateUser(authorization.Replace(BasicAuthResponseHeaderValue, string.Empty).Trim(), isPersistent, context);
				if (!result)
				{
					//context.Response.Headers.Add(BasicAuthResponseHeader, "Basic realm=\"Test\"");
					context.Response.ContentType = httpContentType;
					context.Response.StatusDescription = "Not authorized.";
					context.Response.Write("User wasn't authenticated. Bad authorization data.");
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				}
				else
				{
					context.Response.ContentType = httpContentType;
					context.Response.StatusDescription = "Authorized.";
					context.Response.StatusCode = (int)HttpStatusCode.OK;
					context.Response.Write("User has been authorized.");
				}
				return;
			}
			context.Response.ContentType = httpContentType;
			context.Response.StatusDescription = "Authorizaztion header wasn't specified.";
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			context.Response.Write("Authorization header wasn't specified.");
		}

		private bool AuthenticateUser(string credentials, bool isPersistent, HttpContext context)
		{
			bool validated = false;
			try
			{
				var encoding = Encoding.GetEncoding("UTF-8");
				credentials = encoding.GetString(Convert.FromBase64String(credentials));

				int separator = credentials.IndexOf(':');
				string name = credentials.Substring(0, separator);
				string password = credentials.Substring(separator + 1);

				validated = Login(context, name, password, isPersistent);
			}
			catch (FormatException ex)
			{
				Global.Logger.Error(ex);
				validated = false;
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				validated = false;
			}
			return validated;
		}

		private bool Login(HttpContext context, string strUser, string strPwd, bool isPersistent)
		{
			if (CheckPassword(strUser, strPwd))
			{
				FormsAuthenticationTicket ticket;
				UserDto userDto = null;

				using (IUnityOfWork unity = Infrastructure.BootStrapper.serviceLocator.Get<IUnityOfWork>())
				{
					IReadOnlyRepository<User> repo = unity.GetRepository<User>();
					User validatedUser = repo.Get(x => x.Name == strUser).FirstOrDefault();
					if (validatedUser != null)
						userDto = AutoMapper.Mapper.Map<UserDto>(validatedUser);
				}

				string ticketData = string.Concat("Udelphi|", string.Join(",", userDto.Privileges.Select(p => p.Name)));
				ticket = new FormsAuthenticationTicket(1, strUser, DateTime.Now, DateTime.Now.AddMinutes(30), /*expire time*/ isPersistent,/*persistent*/ ticketData /*user data*/);
				string strEncryptedTicket = FormsAuthentication.Encrypt(ticket);
				HttpCookie cookie = new HttpCookie(Infrastructure.Constants.FormsAuthKey, strEncryptedTicket);
				cookie.HttpOnly = true;//TODO: check cookie settings here!!!
				context.Response.Cookies.Add(cookie);
				IPrincipalProvider provider = BootStrapper.serviceLocator.Get<IPrincipalProvider>();
				System.Security.Principal.IPrincipal principal = provider.CreatePrincipal(ticket);
				if (principal is CustomPrincipal)
				{
					CustomPrincipal p = principal as CustomPrincipal;
					p.UserId = userDto.Id;
					SetPrincipal(p);
				}
				return true;
			}
			else
				return false;
		}

		private bool CheckPassword(string username, string password)
		{
			IEntityValidator validator = Infrastructure.BootStrapper.serviceLocator.Get<IEntityValidator>("CredentialsValidator");
			bool result = false;
			if (validator != null)
			{
				result = validator.IsValid<User>(new User() { Name = username, Password = password });
			}
			return result;
		}

		private void SetPrincipal(Infrastructure.CustomPrincipal principal)
		{
			if (HttpContext.Current != null)
				HttpContext.Current.User = principal;
			System.Threading.Thread.CurrentPrincipal = principal;
		}
	}
}