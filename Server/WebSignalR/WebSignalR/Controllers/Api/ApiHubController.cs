using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Web.Http;

namespace WebSignalR.Controllers.Api
{
	public abstract class ApiHubController<T> : ApiController where T : Hub
	{
		protected IHubConnectionContext Clients { get; private set; }
		protected IGroupManager Groups { get; private set; }

		public ApiHubController()
		{
			var ctx = GlobalHost.ConnectionManager.GetHubContext<T>();
			Clients = ctx.Clients;
			Groups = ctx.Groups;
		}

	}
}