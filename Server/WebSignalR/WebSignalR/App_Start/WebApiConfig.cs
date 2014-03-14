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

			config.DependencyResolver = new DependencyResolvers.NinjectWebApiDependencyResolver(BootStrapper.Kernel);

			#region formatting

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
					//fix issue with self referencing loop detection
					ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
					//if uncommented the line below, in KnockoutJS bindings needs to be refactored to camel case (example: 'name' instead of 'Name', TimeBefore ->timeBefore)
					//ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
				};
			}
			#endregion formatting
			//config.Services.Replace(typeof(System.Web.Http.Dispatcher.IHttpControllerActivator),new Activators.CustomApiActivator());
			config.Services.Replace(typeof(System.Web.Http.Tracing.ITraceWriter), new Infrastructure.DynamicTrace());
		}

	}
}