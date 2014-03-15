using Microsoft.AspNet.SignalR;
using System;
using System.Linq;

namespace WebSignalR.Infrastructure.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class SignalRAuthAttribute : AuthorizeAttribute
	{
		protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
		{
			if (user == null)
				throw new ArgumentNullException("user parameter cannot be a null.");

			CustomPrincipal principal = (user as CustomPrincipal);
			if (principal == null)
				return false;

			if (Roles != null)
			{
				string[] inputRoles = Roles.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				bool notFound = false;
				foreach (string role in inputRoles)
				{
					notFound = !principal.Roles.Any(x => x.Equals(role, StringComparison.OrdinalIgnoreCase));
					if (notFound)
						break;
				}
				return !notFound;
			}
			return principal.Identity.IsAuthenticated;
		}
	}
}