using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebSignalR.DependencyResolvers
{
	internal class NinjectSignalRDependencyResolver : Microsoft.AspNet.SignalR.DefaultDependencyResolver
	{
		private readonly IKernel _kernel;

		public NinjectSignalRDependencyResolver(IKernel kernel)
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