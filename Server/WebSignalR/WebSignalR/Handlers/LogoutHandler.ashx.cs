using System;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebSignalR.Handlers
{
	/// <summary>
	/// Summary description for LogoutHandler
	/// </summary>
	public class LogoutHandler : IHttpHandler, IRequiresSessionState
	{
		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			if (context.Request.IsAuthenticated)
			{
				string[] myCookies = context.Request.Cookies.AllKeys;
				foreach (string cookie in myCookies)
					context.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);

				context.Session.Abandon();
				FormsAuthentication.SignOut();
				//context.Response.Redirect();
			}
			else
			{
				context.Response.ContentType = "text/plain";
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.StatusDescription = "Froms authentication wasn't specified.";
				context.Response.Write("Froms authentication wasn't specified.");
			}
		}
	}
}