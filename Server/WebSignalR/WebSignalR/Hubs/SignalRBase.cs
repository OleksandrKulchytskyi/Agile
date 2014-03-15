using Microsoft.AspNet.SignalR;
using System;

namespace WebSignalR.Hubs
{
	public abstract class SignalRBase<T> where T : Hub
	{
		private Lazy<IHubContext> hubLazy =
			new Lazy<IHubContext>(() => Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<T>());

		public IHubContext HubInstance
		{
			get { return hubLazy.Value; }
		}
	}
}