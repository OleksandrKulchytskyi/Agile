using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Infrastructure.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class SignalRAuthAttribute : AuthorizeAttribute
	{
		protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
		{
			if (user == null)
				throw new ArgumentNullException("user parameter cannot be a null.");

			System.Security.Principal.GenericPrincipal principal = (System.Security.Principal.GenericPrincipal)user;
			if (principal != null)
			{
				return principal.Identity.IsAuthenticated;
			}
			return false;
		}
	}
}