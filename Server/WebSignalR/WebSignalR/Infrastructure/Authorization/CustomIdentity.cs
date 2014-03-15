using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;

namespace WebSignalR.Infrastructure
{
	public class CustomIdentity : System.Security.Principal.IIdentity
	{
		private FormsAuthenticationTicket _ticket;

		public CustomIdentity(FormsAuthenticationTicket ticket)
		{
			_ticket = ticket;
		}

		public FormsAuthenticationTicket Ticket
		{
			get { return _ticket; }
		}

		public string AuthenticationType
		{
			get { return "FormsCustom"; }
		}

		public bool IsAuthenticated
		{
			get { return true; }
		}

		public string Name
		{
			get { return _ticket.Name; }
		}

		public string CompanyName
		{
			get
			{
				if (!string.IsNullOrEmpty(_ticket.UserData))
				{
					string[] pieces = _ticket.UserData.Split("|".ToCharArray());
					return pieces[0];
				}
				return string.Empty;
			}
		}

		public bool IsAdmin
		{
			get
			{
				if (!string.IsNullOrEmpty(_ticket.UserData))
				{
					string[] pieces = _ticket.UserData.Split("|".ToCharArray());
					return (pieces[1].IndexOf("admin", StringComparison.OrdinalIgnoreCase) != -1);
				}
				return false;
			}
		}

		public List<String> Roles
		{
			get
			{
				if (!string.IsNullOrEmpty(_ticket.UserData))
				{
					string[] pieces = _ticket.UserData.Split("|".ToCharArray());
					return pieces[1].Split(",".ToCharArray()).ToList();
				}
				return new List<string>();
			}
		}
	}
}