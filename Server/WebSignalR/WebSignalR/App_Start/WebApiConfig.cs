using System;
using System.Linq;
using System.Web.Http;
using WebSignalR.Infrastructure;

namespace WebSignalR
{
	public class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
			config.Routes.MapHttpRoute(name: "DefaultApiAction",
				routeTemplate: "api/{controller}/{action}",
				defaults: new { controller = "room", action = "getRooms" }
			);
			config.Routes.MapHttpRoute(name: "DefaultApiActionParam",
				routeTemplate: "api/{controller}/{action}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			config.DependencyResolver = new DependencyResolvers.NinjectWebApiDependencyResolver(BootStrapper.serviceLocator.Kernel);
			//config.Services.Replace(typeof(System.Web.Http.Dispatcher.IHttpControllerActivator),new Activators.CustomApiActivator());
			if (System.Configuration.ConfigurationManager.AppSettings["Response.Compression"].IndexOf("true") != -1)
				config.MessageHandlers.Insert(0, new WebSignalR.Infrastructure.Handlers.CompressionHandler()); // first runs last

			config.MessageHandlers.Add(new Infrastructure.Handlers.EmptyPostBodyMessageHandler());
			config.Filters.Add(new Infrastructure.Filters.WebApiExceptionFilter());
			config.Services.Replace(typeof(System.Web.Http.Tracing.ITraceWriter), new Infrastructure.DynamicTrace());

			System.Web.Http.Dispatcher.IAssembliesResolver assemblyResolver = new CustomWebApiAssemblyResolver();
			config.Services.Replace(typeof(System.Web.Http.Dispatcher.IAssembliesResolver), assemblyResolver);


			#region Formatting

			var xmlFormatter = GlobalConfiguration.Configuration.Formatters.Where(f =>
			{
				return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/xml", StringComparison.CurrentCultureIgnoreCase));
			}).FirstOrDefault() as System.Net.Http.Formatting.JsonMediaTypeFormatter;
			if (xmlFormatter != null)
			{
				GlobalConfiguration.Configuration.Formatters.Remove(xmlFormatter);
				xmlFormatter = null;
			}

			var formatter = GlobalConfiguration.Configuration.Formatters.Where(f =>
			{
				return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/json", StringComparison.CurrentCultureIgnoreCase));
			}).FirstOrDefault() as System.Net.Http.Formatting.JsonMediaTypeFormatter;

			if (formatter != null)
			{
				formatter.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
				{
					NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
					Converters = new Newtonsoft.Json.JsonConverter[] { new Newtonsoft.Json.Converters.IsoDateTimeConverter() },
					DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat,
					ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,//fix the issue with self referencing loop detection
					//if uncommented the line below, in KnockoutJS bindings needs to be refactored to camel case (example: 'name' instead of 'Name', TimeBefore ->timeBefore)
					//ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
				};
			}
			config.Formatters.Add(new Infrastructure.Formatters.CsvMediaTypeFormatter());
			#endregion formatting
		}
	}
}