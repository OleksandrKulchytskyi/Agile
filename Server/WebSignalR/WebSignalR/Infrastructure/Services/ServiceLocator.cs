using Ninject;
using System;
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
			private readonly IKernel kernel;  // Ninject kernel

			public DefaultServiceLocator()
			{
				kernel = new StandardKernel();
			}

			public T Get<T>()
			{
				return kernel.Get<T>();
			}

			public IKernel Kernel
			{
				get { return kernel; }
			}

			public void InitBindings(Action<IKernel> initializer)
			{
				if (initializer != null)
					initializer(kernel);
			}

			public T Get<T>(string named)
			{
				return kernel.Get<T>(named);
			}
		}
	}
}