using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IPrincipalProvider
	{
		IPrincipal CreatePrincipal(object context);
	}
}