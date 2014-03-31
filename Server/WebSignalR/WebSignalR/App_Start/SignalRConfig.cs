﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Web.Routing;

namespace WebSignalR.App_Start
{
	public static class SignalRConfig
	{
		public static void Register(IDependencyResolver resolver)
		{
			HubConfiguration hubConfig = new HubConfiguration
			{
				Resolver = resolver,
				EnableJavaScriptProxies = true,
				EnableDetailedErrors = true
			};

			if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["Redis.Enable"]))
			{
				string server = System.Configuration.ConfigurationManager.AppSettings["Redis.Server"];
				string port = System.Configuration.ConfigurationManager.AppSettings["Redis.Port"];
				string password = System.Configuration.ConfigurationManager.AppSettings["Redis.Password"];
				var scaleout = new RedisScaleoutConfiguration(server, int.Parse(port), password, "WebSignalR");
				GlobalHost.DependencyResolver.UseRedis(scaleout);
			}

			RouteTable.Routes.MapHubs(hubConfig);

			#region temp validation
			//try
			//{
			//	object bus = GlobalHost.DependencyResolver.Resolve<Microsoft.AspNet.SignalR.Messaging.IMessageBus>();
			//	if (bus != null)
			//	{

			//	}
			//}
			//catch (Exception)
			//{

			//	throw;
			//} 
			#endregion

			IConfigurationManager configManager = resolver.Resolve<IConfigurationManager>();
			configManager.ConnectionTimeout = TimeSpan.FromSeconds(30);
			configManager.DisconnectTimeout = TimeSpan.FromSeconds(30);

			IHubPipeline hubPipeline = resolver.Resolve<IHubPipeline>();
			hubPipeline.AddModule(new Hubs.Pipelines.LogErrorPipelineModule());

			System.Web.Http.GlobalConfiguration.Configuration.Formatters.JsonFormatter.
				SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

			//this was disappear since v 1.0,this feature is turned on by defult in v1.0 >
			//pipeline.EnableAutoRejoiningGroups();
			//RouteTable.Routes.MapHubs();
		}
	}
}