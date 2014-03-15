using System;
using System.Security.Principal;
using System.Web.Security;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Services
{
	public class FormsPrincipalProvider : IPrincipalProvider
	{
		public IPrincipal CreatePrincipal(object context)
		{
			if (context == null)
				throw new ArgumentNullException("Context cannot be a null.");

			FormsAuthenticationTicket ticket = null;

			if (context is FormsAuthenticationTicket)
				ticket = (context as FormsAuthenticationTicket);

			else if (context is FormsIdentity)
			{
				FormsIdentity fIdent = context as FormsIdentity;
				ticket = fIdent.Ticket;
			}

			if (ticket == null) return null;

			CustomIdentity customIdentity = new CustomIdentity(ticket); // Create a CustomIdentity based on the FormsAuthenticationTicket
			CustomPrincipal principal = new CustomPrincipal(customIdentity); // Create the CustomPrincipal
			return principal;
		}
	}
}