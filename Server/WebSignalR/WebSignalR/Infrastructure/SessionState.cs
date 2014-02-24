using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Infrastructure
{
	[Serializable]
	public class SessionState
	{
		public int? UserId { get; set; }
		public string SessionId { get; set; }
	}
}