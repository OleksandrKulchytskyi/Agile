using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace WebSignalR
{
	public class Global : System.Web.HttpApplication
	{

		public static log4net.ILog Logger = null;

		protected void Application_Start(object sender, EventArgs e)
		{
			Logger = LogManager.GetLogger(typeof(Global).FullName);
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Application_Start()");

		

			Microsoft.AspNet.SignalR.GlobalHost.HubPipeline.AddModule(new Hubs.Pipelines.LoggingPipelineModule());

			
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

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

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{
			Logger.Info("Application_End()");
			LogManager.Shutdown();
		}
	}
}