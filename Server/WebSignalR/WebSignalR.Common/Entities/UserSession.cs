using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Entities
{
	public class UserSession : EntityBase
	{
		[Required]
		public string SessionId { get; set; }
		public User User { get; set; }
		public DateTimeOffset LastActivity { get; set; }

		public string UserAgent { get; set; }
		public int UserId { get; set; }
	}
}