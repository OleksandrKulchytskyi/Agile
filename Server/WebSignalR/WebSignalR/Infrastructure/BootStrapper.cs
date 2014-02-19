using Microsoft.AspNet.SignalR;
using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebSignalR.Infrastructure.BootStrapper), "PreAppStart")]
namespace WebSignalR.Infrastructure
{
	public static class BootStrapper
	{
		private static bool _sweeping;
		private static bool _broadcasting;

		private static readonly TimeSpan _sweepInterval = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan _broadcatsInterval = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["broadcastTime"]));
		private const string SqlClient = "System.Data.SqlClient";

		internal static IKernel Kernel = null;
		private static IDependencyResolver _resolver = null;

		public static void PreAppStart()
		{
			var kernel = new StandardKernel();

			var serializer = new Microsoft.AspNet.SignalR.Json.JsonNetSerializer(new Newtonsoft.Json.JsonSerializerSettings
			{
				DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat,
			});

			Kernel = kernel;
			_resolver = new DependencyResolvers.NinjectDependencyResolver(kernel);
			App_Start.SignalRConfig.Register(_resolver);
		}

	}
}