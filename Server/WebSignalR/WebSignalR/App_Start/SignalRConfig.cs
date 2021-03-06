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
				var crypto = Infrastructure.BootStrapper.serviceLocator.Get<WebSignalR.Common.Interfaces.ICrypto>();
				string server = System.Configuration.ConfigurationManager.AppSettings["Redis.Server"];
				string port = System.Configuration.ConfigurationManager.AppSettings["Redis.Port"];
				string password = crypto.Decrypt(System.Configuration.ConfigurationManager.AppSettings["Redis.Password"]);
				var scaleout = new RedisScaleoutConfiguration(server, int.Parse(port), password, "WebSignalR");
				GlobalHost.DependencyResolver.UseRedis(scaleout);
			}

			RouteTable.Routes.MapHubs(hubConfig);

			IConfigurationManager configManager = resolver.Resolve<IConfigurationManager>();
			configManager.ConnectionTimeout = TimeSpan.FromSeconds(30);
			configManager.DisconnectTimeout = TimeSpan.FromSeconds(30);

			IHubPipeline hubPipeline = resolver.Resolve<IHubPipeline>();
			hubPipeline.AddModule(new Hubs.Pipelines.LogErrorPipelineModule());

			System.Web.Http.GlobalConfiguration.Configuration.Formatters.JsonFormatter.
				SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
		}
	}
}