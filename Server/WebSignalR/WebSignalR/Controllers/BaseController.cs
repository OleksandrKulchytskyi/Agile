using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
	}
}