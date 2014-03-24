using Ninject;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace WebSignalR.DependencyResolvers
{
	/// <summary>
	/// Sets up the resolution for MVC controllers
	/// </summary>
	public class NInjectMvcDependencyResolver : IDependencyResolver
	{
		private IKernel _kernel;

		public NInjectMvcDependencyResolver(IKernel kernel)
		{
			_kernel = kernel;
		}

		public object GetService(Type serviceType)
		{
			return _kernel.TryGet(serviceType) ?? null;
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _kernel.GetAll(serviceType) ?? new List<object>();
		}
	}

	/// <summary>
	/// Activates MVC controllers.
	/// </summary>
	public class NinjectMvcControllerActivator : IControllerActivator
	{
		public IController Create(System.Web.Routing.RequestContext requestContext, Type controllerType)
		{
			var ret = DependencyResolver.Current.GetService(controllerType) as IController;
			return ret;
		}
	}
}