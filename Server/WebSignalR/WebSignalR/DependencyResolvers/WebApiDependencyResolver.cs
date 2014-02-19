using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;

namespace WebSignalR.DependencyResolvers
{
	public class NinjectDependencyScope : IDependencyScope
	{
		private IResolutionRoot resolutionRoot;
		private bool _dispoded = false;

		internal NinjectDependencyScope(IResolutionRoot kernel)
		{
			if (kernel == null)
				throw new ArgumentNullException("kernel parameter cannot be a null.");
			this.resolutionRoot = kernel;
		}

		public object GetService(Type serviceType)
		{
			if (resolutionRoot == null && _dispoded)
				throw new ObjectDisposedException("this", "This scope has already been disposed");

			return resolutionRoot.Resolve(this.CreateRequest(serviceType)).FirstOrDefault();

			//return resolutionRoot.TryGet(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			if (resolutionRoot == null && _dispoded)
				throw new ObjectDisposedException("this", "This scope has already been disposed");

			return resolutionRoot.Resolve(this.CreateRequest(serviceType)).ToList();

			//return resolutionRoot.GetAll(serviceType);
		}

		private IRequest CreateRequest(Type reqType)
		{
			return resolutionRoot.CreateRequest(reqType, null, new Parameter[0], true, true);
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_dispoded)
			{
				_dispoded = true;

				if (disposing)
				{
					IDisposable disposable = resolutionRoot as IDisposable;
					if (disposable != null && !_dispoded)
					{
						disposable.Dispose();
						resolutionRoot = null;
					}
				}
			}
		}

		#endregion IDisposable
	}

	public class NinjectWebApiDependencyResolver : NinjectDependencyScope, IDependencyResolver
	{
		private IKernel _kernel;

		public NinjectWebApiDependencyResolver(IKernel kernel)
			: base(kernel)
		{
			this._kernel = kernel;
		}

		public IDependencyScope BeginScope()
		{
			return new NinjectDependencyScope(_kernel.BeginBlock());
		}
	}
}