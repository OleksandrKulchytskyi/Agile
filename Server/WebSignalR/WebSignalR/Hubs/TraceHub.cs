using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Hubs
{
	[Authorize(Roles = "Admin,Administrator")]
	[HubName("traceHub")]
	public class TraceHub : Hub
	{
		public void UserJoin()
		{
			//TODO: impelement some tracing storage and retreive data from that to display on client side
			List<string> traceData = new List<string>();
			Clients.Caller.UserJoined(traceData, Context.ConnectionId);
		}
	}
}