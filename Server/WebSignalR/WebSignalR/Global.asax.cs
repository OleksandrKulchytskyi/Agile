using log4net;
using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebSignalR.Common.Interfaces;
using WebSignalR.Infrastructure;

namespace WebSignalR
{
	public class Global : HttpApplication
	{
		public static log4net.ILog Logger = null;

		protected void Application_Start(object sender, EventArgs e)
		{
			Logger = LogManager.GetLogger(typeof(Global).FullName);
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Application_Start()");

			//Infrastructure.BootStrapper.DoMigrations("ConnectionSettings");
			Infrastructure.BootStrapper.InitMapperMaps();
			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Error("UnhandledException", e.ExceptionObject as Exception);
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			if (this.Request.Path.EndsWith("/handlers/loginhandler.ashx", StringComparison.OrdinalIgnoreCase)
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
			IPrincipal usr = HttpContext.Current.User; // If we are dealing with an authenticated forms authentication request
			if (usr.Identity.IsAuthenticated && usr.Identity.AuthenticationType == "Forms")
			{
				var provider = BootStrapper.serviceLocator.Get<IPrincipalProvider>();
				IPrincipal p = provider.CreatePrincipal(usr.Identity as FormsIdentity);
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
				//Logger.Error("Application_Error", ex);
			}
			else
				Logger.Error("Application_Error", exception);
		}

		protected void Application_End(object sender, EventArgs e)
		{
			BootStrapper.serviceLocator.Get<IPurge>().Dispose();
			Logger.Info("Application_End()");
			LogManager.Shutdown();
		}
	}
}