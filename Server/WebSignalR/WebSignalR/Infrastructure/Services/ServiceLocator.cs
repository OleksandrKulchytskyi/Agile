using Ninject;
using System;
using System.Linq;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Services
{
	internal class ServiceLocator
	{
		private static IServiceLocator serviceLocator;

		static ServiceLocator()
		{
			serviceLocator = new DefaultServiceLocator();//initializing default service locator based on NInject
		}

		public static IServiceLocator Default
		{
			get { return serviceLocator; }
		}

		private class DefaultServiceLocator : IServiceLocator
		{
			private readonly IKernel kernel;

			public DefaultServiceLocator()
			{
				kernel = new StandardKernel();
				//kernel.Components.Add<Ninject.Planning.Bindings.Resolvers.IMissingBindingResolver, DefaultImplBindingResolver>();
			}

			public IKernel Kernel
			{
				get { return kernel; }
			}

			public void LoadModule(string path)
			{
				kernel.Load(resolvePath(path));
			}

			private string resolvePath(string path)
			{
				string compounded = string.Empty;
				if (System.Web.HttpContext.Current != null)
					compounded = System.Web.HttpContext.Current.Server.MapPath(path);
				else
					compounded = System.Web.Hosting.HostingEnvironment.MapPath(path);
				return compounded;
			}

			public void InitBindings(Action<IKernel> initializer)
			{
				if (initializer != null)
					initializer(kernel);
			}

			public T Get<T>()
			{
				return kernel.Get<T>();
			}

			public T Get<T>(string named)
			{
				return kernel.Get<T>(named);
			}
		}

		private class DefaultImplBindingResolver : Ninject.Components.NinjectComponent, Ninject.Planning.Bindings.Resolvers.IMissingBindingResolver
		{
			public System.Collections.Generic.IEnumerable<Ninject.Planning.Bindings.IBinding> Resolve(Ninject.Infrastructure.Multimap<Type, Ninject.Planning.Bindings.IBinding> bindings, Ninject.Activation.IRequest request)
			{
				var service = request.Service;
				if (!service.IsInterface || !service.Name.StartsWith("I"))
					return Enumerable.Empty<Ninject.Planning.Bindings.IBinding>();

				return new[] { new Ninject.Planning.Bindings.Binding(service) { ProviderCallback = Ninject.Activation.Providers.StandardProvider.GetCreationCallback(GetDefaultImplementation(service)) } };
			}

			private Type GetDefaultImplementation(Type service)
			{
				var typeName = string.Format("{0}.{1}", service.Namespace, service.Name.TrimStart('I'));
				return Type.GetType(typeName);
			}
		}
	}
}