using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WebSignalR.Hubs.Pipelines
{
	public class LoggingPipelineModule : HubPipelineModule
	{
		protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
		{
#if DEBUG
			Debug.WriteLine("=> Invoking " + context.MethodDescriptor.Name + " on hub " + context.MethodDescriptor.Hub.Name);
#endif
			return base.OnBeforeIncoming(context);
		}

		protected override bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
		{
#if DEBUG
			Debug.WriteLine("<= Invoking " + context.Invocation.Method + " on client hub " + context.Invocation.Hub);
#endif
			return base.OnBeforeOutgoing(context);
		}
	}
}