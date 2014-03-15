using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Ninject;
using System.Web.Http;
using WebSignalR.Hubs;

namespace WebSignalR.Controllers
{
	public abstract class BaseController : ApiController
	{
		protected IHubContext GetHub<THub>() where THub : Hub
		{
			return GlobalHost.ConnectionManager.GetHubContext<THub>();
		}

		public IHubContext AgileHubContext
		{
			get
			{
				return GetHub<AgileHub>();
			}
		}

		public IHubConnectionContext AgileHubConnection
		{
			get
			{
				return Infrastructure.BootStrapper.Kernel.Get<IHubConnectionContext>("AgileHub");
			}
		}
	}
}