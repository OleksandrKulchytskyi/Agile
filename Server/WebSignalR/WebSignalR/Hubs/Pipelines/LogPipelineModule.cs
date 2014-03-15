using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Diagnostics;

namespace WebSignalR.Hubs.Pipelines
{
	public class LogErrorPipelineModule : HubPipelineModule
	{
		protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
		{
#if DEBUG
			if (context.MethodDescriptor.Hub != null && context.MethodDescriptor.Hub.Name != null && context.MethodDescriptor.Name != null)
				Debug.WriteLine("=> Invoking " + context.MethodDescriptor.Name + " on hub " + context.MethodDescriptor.Hub == null ? string.Empty : context.MethodDescriptor.Hub.Name);
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

		protected override void OnIncomingError(Exception ex, IHubIncomingInvokerContext context)
		{
			Global.Logger.Error(ex);
			Debug.WriteLine("HUB => Exception " + ex.Message);
			if (ex.InnerException != null)
			{
				Debug.WriteLine("HUB => Inner Exception " + ex.InnerException.Message);
			}
			base.OnIncomingError(ex, context);
		}
	}
}