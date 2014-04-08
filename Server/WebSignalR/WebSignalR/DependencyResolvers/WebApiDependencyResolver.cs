using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using WebSignalR.Common.Extension;

namespace WebSignalR.DependencyResolvers
{
	public class NinjectDependencyScope : IDependencyScope
	{
		const int NOTDISPOSED = 0;
		const int DISPOSED = 1;

		private readonly IResolutionRoot resolutionRoot;
		private int _state = NOTDISPOSED;

		internal NinjectDependencyScope(IResolutionRoot kernel)
		{
			Ensure.Argument.NotNull(kernel, "kernel");
			this.resolutionRoot = kernel;
		}

		public object GetService(Type serviceType)
		{
			if (IsDisposed())
				throw new ObjectDisposedException("this", "This scope has already been disposed");

			return resolutionRoot.Resolve(this.CreateRequest(serviceType)).FirstOrDefault();
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			if (IsDisposed())
				throw new ObjectDisposedException("this", "This scope has already been disposed");

			return resolutionRoot.Resolve(this.CreateRequest(serviceType)).ToList();
		}

		private bool IsDisposed()
		{
			return (System.Threading.Interlocked.CompareExchange(ref _state, DISPOSED, DISPOSED) == DISPOSED);
		}

		private IRequest CreateRequest(Type reqType)
		{
			return resolutionRoot.CreateRequest(reqType, null, new Parameter[0], true, true);
		}

		private bool TypeIsSystemAssembly(Type service)
		{
			if (service.Namespace != null)
			{
				return service.Namespace.StartsWith("System", StringComparison.InvariantCultureIgnoreCase);
			}

			return false;
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (System.Threading.Interlocked.Exchange(ref _state, DISPOSED) == NOTDISPOSED)
			{
				if (disposing)
				{
					//Omit below since we do not have to dispose single IKernel object
					//IDisposable disposable = resolutionRoot as IDisposable;
					//if (disposable != null)
					//{
					//	disposable.Dispose();
					//}
				}
			}
		}
		#endregion IDisposable
	}

	public class NinjectWebApiDependencyResolver : NinjectDependencyScope, IDependencyResolver
	{
		private readonly IKernel _kernel;

		public NinjectWebApiDependencyResolver(IKernel kernel)
			: base(kernel)
		{
			this._kernel = kernel;
		}

		public IDependencyScope BeginScope()
		{
			//Omit below since we do not have stable singleton instances in WebApi
			//return new NinjectDependencyScope(_kernel.BeginBlock());
			return new NinjectDependencyScope(_kernel);
		}
	}
}