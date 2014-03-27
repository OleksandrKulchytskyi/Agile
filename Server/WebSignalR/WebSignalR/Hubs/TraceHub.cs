using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;

namespace WebSignalR.Hubs
{
	[WebSignalR.Infrastructure.Authorization.SignalRAuth(Roles = "Admin")]
	[HubName("traceHub")]
	public class TraceHub : Hub
	{
		public void UserJoin()
		{
			//TODO: impelement some tracing storage and retreive data from that to display on client side
			List<string> traceData = new List<string>();
			Clients.Caller.onUserJoined(traceData, Context.ConnectionId);
		}
	}
}