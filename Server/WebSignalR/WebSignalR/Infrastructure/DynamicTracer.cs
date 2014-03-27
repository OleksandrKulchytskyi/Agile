using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebSignalR.Infrastructure
{
	public class DynamicTracer : Hubs.SignalRBase<Hubs.TraceHub>
	{
		public void LogToHub(Exception ex)
		{
			if (ex == null)
				return;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(ex.Message);
			if (ex.InnerException != null)
				sb.AppendLine(ex.InnerException.Message);

			sb.AppendLine(ex.ToString());

			if (HubInstance != null)
				HubInstance.Clients.All.logTraceMsg(sb.ToString()).Wait();
		}
	}
}