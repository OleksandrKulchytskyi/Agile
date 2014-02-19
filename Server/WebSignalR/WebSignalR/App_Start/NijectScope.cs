using Ninject.Activation;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;

namespace WebSignalR.App_Start
{
	public class NinjectScope : IDependencyScope
	{
		protected IResolutionRoot resolutionRoot;
		private bool _disposed = false;

		public NinjectScope(IResolutionRoot kernel)
		{
			resolutionRoot = kernel;
		}

		public object GetService(Type serviceType)
		{
			IRequest request = resolutionRoot.CreateRequest(serviceType, null, new Ninject.Parameters.Parameter[0], true, true);
			return resolutionRoot.Resolve(request).SingleOrDefault();
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			IRequest request = resolutionRoot.CreateRequest(serviceType, null, new Ninject.Parameters.Parameter[0], true, true);
			return resolutionRoot.Resolve(request).ToList();
		}

		[DebuggerStepThrough]
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					IDisposable disposable = (IDisposable)resolutionRoot;
					if (disposable != null)
						disposable.Dispose();

					resolutionRoot = null;
					GC.Collect();
				}
				_disposed = true;
			}
		}
	}
}