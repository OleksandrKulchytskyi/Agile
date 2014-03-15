using System;

namespace WebSignalR.Infrastructure
{
	[Serializable]
	public class SessionState
	{
		public int? UserId { get; set; }

		public string SessionId { get; set; }
	}
}