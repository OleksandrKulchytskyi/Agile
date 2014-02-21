using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Hubs
{
	[Microsoft.AspNet.SignalR.Hubs.HubName("testhub")]
	public class TestHub : Hub
	{
		private static readonly Version _version = typeof(TestHub).Assembly.GetName().Version;
		private IUnityOfWork _unity;
		private ICryptoService _cryptoService;

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

		public TestHub(IUnityOfWork unity, ICryptoService crypto)
		{
			_unity = unity;
			_cryptoService = crypto;
		}

		public void Hello()
		{
			Clients.All.hello(new TestData() { Id = 1, Name = "Test" });
		}

		public override System.Threading.Tasks.Task OnConnected()
		{
			if (Context.User.Identity.IsAuthenticated)
			{
			}

			Clients.Caller.hello(new TestData() { Id = 1, Name = "Test" });
			return base.OnConnected();
		}

		public override System.Threading.Tasks.Task OnDisconnected()
		{
			return base.OnDisconnected();
		}

		public override System.Threading.Tasks.Task OnReconnected()
		{
			return base.OnReconnected();
		}
	}

	public class TestData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}