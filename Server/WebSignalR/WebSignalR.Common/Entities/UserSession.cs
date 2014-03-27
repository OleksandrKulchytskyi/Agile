using System;
using System.ComponentModel.DataAnnotations;

namespace WebSignalR.Common.Entities
{
	public class UserSession : EntityBase
	{
		[Required]
		public string SessionId { get; set; }

		public DateTimeOffset LastActivity { get; set; }

		public string UserAgent { get; set; }

		public int? UserId { get; set; }

		public virtual User User { get; set; }

		public SessionRoom SessionRoom { get; set; }

		public void SetLastActivityNow()
		{
			LastActivity = DateTime.UtcNow;
		}
	}
}