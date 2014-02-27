using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using WebSignalR.Infrastructure;

namespace WebSignalR
{
	public class Global : HttpApplication
	{
		private static List<string> _sessionInfo;
		private static readonly object padlock = new object();
		public static log4net.ILog Logger = null;

		public static List<string> Sessions
		{
			get
			{
				lock (padlock)
				{
					if (_sessionInfo == null)
					{
						_sessionInfo = new List<string>();
					}
					return _sessionInfo;
				}
			}
		}

		protected void Application_Start(object sender, EventArgs e)
		{
			Logger = LogManager.GetLogger(typeof(Global).FullName);
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Application_Start()");

			//Infrastructure.BootStrapper.DoMigrations("ConnectionSettings");
			Infrastructure.BootStrapper.InitMapperMaps();
			WebApiConfig.Register(GlobalConfiguration.Configuration);

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Error("UnhandledException", e.ExceptionObject as Exception);
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			Sessions.Add(Session.SessionID);
		}

		protected void Session_End(object sender, EventArgs e)
		{
			Sessions.Remove(Session.SessionID);
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			if (this.Request.Path.ToLower().StartsWith("/handlers/loginhandler.ashx")
			  && this.Response.StatusCode == 302
			  && this.Response.RedirectLocation.ToLower().Contains("login.aspx"))
			{
				this.Response.RedirectLocation = "/handlers/loginhandler.ashx";
				this.Response.StatusCode = 401;
			}
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected void Application_OnPostAuthenticateRequest(object sender, EventArgs e)
		{
			IPrincipal usr = HttpContext.Current.User; // Create the CustomPrincipal
			// If we are dealing with an authenticated forms authentication request
			if (usr.Identity.IsAuthenticated && usr.Identity.AuthenticationType == "Forms")
			{
				FormsIdentity fIdent = usr.Identity as FormsIdentity;
				CustomIdentity ci = new CustomIdentity(fIdent.Ticket); // Create a CustomIdentity based on the FormsAuthenticationTicket  
				CustomPrincipal p = new CustomPrincipal(ci); // Create the CustomPrincipal
				HttpContext.Current.User = p;
				Thread.CurrentPrincipal = p;
			}
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			Exception exception = Server.GetLastError();
			HttpException ex = exception as HttpException;

			if (ex != null)
			{
				var filePath = Context.Request.FilePath;
				var url = ((HttpApplication)sender).Context.Request.Url;
				Logger.Warn("URL: " + url + "; FilePath: " + filePath);
				Logger.Error("Application_Error", ex);
			}
			else
				Logger.Error("Application_Error", exception);
		}

		protected void Application_End(object sender, EventArgs e)
		{
			Logger.Info("Application_End()");
			LogManager.Shutdown();
		}
	}
}