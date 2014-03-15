using Microsoft.AspNet.SignalR.Hubs;
using Ninject;
using System.Web.Mvc;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Controllers
{
	public abstract class MvcController : Controller
	{
		protected IUnityOfWork _unity;

		protected IUnityOfWork Unity
		{
			get { return _unity; }
		}

		protected IHubConnectionContext AgileHubConnection
		{
			get
			{
				return Infrastructure.BootStrapper.Kernel.Get<IHubConnectionContext>("AgileHub");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();

			base.Dispose(disposing);
		}
	}
}