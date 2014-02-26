using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace WebSignalR.Infrastructure
{
	public class CustomPrincipal : IPrincipal
	{
		public IIdentity Identity { get; private set; }

		public CustomPrincipal(string Username)
		{
			this.Identity = new GenericIdentity(Username);
		}

		public CustomPrincipal(string Username, bool isAuthenticated)
			: this(Username)
		{
			Authenticated = isAuthenticated;
		}

		public bool IsInRole(string role)
		{
			if (Roles == null)
				return false;
			return Roles.Any(r => role.Contains(r));
		}

		public bool Authenticated { get; set; }
		public int UserId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<string> Roles { get; set; }
	}
}