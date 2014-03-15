using System.Security.Principal;

namespace WebSignalR.Common.Interfaces
{
	public interface IPrincipalProvider
	{
		IPrincipal CreatePrincipal(object context);
	}
}