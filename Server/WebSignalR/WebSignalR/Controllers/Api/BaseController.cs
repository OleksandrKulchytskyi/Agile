using Microsoft.AspNet.SignalR.Hubs;
using Ninject;
using System.Web.Http;

namespace WebSignalR.Controllers
{
	public abstract class BaseController : ApiController
	{
		protected IHubConnectionContext AgileHubConnection
		{
			get
			{
				return Infrastructure.BootStrapper.serviceLocator.Get<IHubConnectionContext>("AgileHub");
			}
		}
	}
}