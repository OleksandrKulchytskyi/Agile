using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Controllers
{
	public abstract class MvcController : Controller
	{
		protected IUnityOfWork _unity;

		//public MvcController(IUnityOfWork unity)
		//{
		//	_unity = unity;
		//}

		protected IUnityOfWork Unity
		{
			get { return _unity; }
		}

		protected IHubContext GetHub<THub>() where THub : Hub
		{
			return GlobalHost.ConnectionManager.GetHubContext<THub>();
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();

			base.Dispose(disposing);
		}
	}
}