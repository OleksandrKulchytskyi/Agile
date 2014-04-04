using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace WebSignalR.Infrastructure
{
	public class CustomWebApiAssemblyResolver : DefaultAssembliesResolver
	{
		public CustomWebApiAssemblyResolver()
		{
			var asmblies = GetAssemblies();
			if (asmblies != null)
			{
			}
		}

		public override ICollection<Assembly> GetAssemblies()
		{
			ICollection<Assembly> baseAssemblies = base.GetAssemblies();

			List<Assembly> assemblies = new List<Assembly>(baseAssemblies);
			foreach (var item in WebSignalR.PluginManager.PluginManager.Current.GetModulesWithAssemblies())
			{
				baseAssemblies.Add(item.Value);
			}
			return assemblies;
		}
	}
}