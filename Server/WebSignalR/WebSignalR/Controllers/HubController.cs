using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Web.Mvc;

namespace WebSignalR.Controllers
{
	public class HubController<T> : Controller where T : Hub
	{
		protected IHubConnectionContext Clients { get; private set; }
		protected IGroupManager Groups { get; private set; }

		public HubController()
		{
			var ctx = GlobalHost.ConnectionManager.GetHubContext<T>();
			Clients = ctx.Clients;
			Groups = ctx.Groups;
		}
	}
}