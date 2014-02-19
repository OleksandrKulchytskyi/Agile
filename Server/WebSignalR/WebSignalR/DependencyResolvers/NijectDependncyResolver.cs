using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.DependencyResolvers
{
	internal class NinjectDependencyResolver : Microsoft.AspNet.SignalR.DefaultDependencyResolver
	{
		private readonly IKernel _kernel;
		public NinjectDependencyResolver(IKernel kernel)
		{
			_kernel = kernel;
		}

		public override object GetService(Type serviceType)
		{
			return _kernel.TryGet(serviceType) ?? base.GetService(serviceType);
		}

		public override IEnumerable<object> GetServices(Type serviceType)
		{
			return _kernel.GetAll(serviceType).Concat(base.GetServices(serviceType));
		}
	}
}