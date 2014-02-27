using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace WebSignalR.Infrastructure
{
	public class CustomPrincipal : IPrincipal
	{
		private CustomIdentity _identity;

		public CustomPrincipal(CustomIdentity identity)
		{
			_identity = identity;
			Roles = _identity.Roles;
		}

		public System.Security.Principal.IIdentity Identity
		{
			get { return _identity; }
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