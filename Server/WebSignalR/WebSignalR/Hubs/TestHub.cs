using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR
{
	[Microsoft.AspNet.SignalR.Hubs.HubName("testhub")]
	public class TestHub : Hub
	{
		private static readonly Version _version = typeof(TestHub).Assembly.GetName().Version;

		private string UserAgent
		{
			get
			{
				if (Context.Headers != null)
					return Context.Headers["User-Agent"];
				return null;
			}
		}

		private bool OutOfSync
		{
			get
			{
				string version = Clients.Caller.version;
				return string.IsNullOrEmpty(version) || new Version(version) != _version;
			}
		}

		public void Hello()
		{
			Clients.All.hello(new TestData() { Id = 1, Name = "Test" });
		}

		public override System.Threading.Tasks.Task OnConnected()
		{
			Clients.Caller.hello(new TestData() { Id = 1, Name = "Test" });
			return base.OnConnected();
		}

		public override System.Threading.Tasks.Task OnDisconnected()
		{
			return base.OnDisconnected();
		}
	}

	public class TestData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}