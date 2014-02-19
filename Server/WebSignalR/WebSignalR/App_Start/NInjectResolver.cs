using Microsoft.AspNet.SignalR;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;

namespace WebSignalR.App_Start
{
	public class NinjectResolver : NinjectScope, System.Web.Http.Dependencies.IDependencyResolver
	{
		private IKernel _kernel;

		public NinjectResolver(IKernel kernel)
			: base(kernel)
		{
			_kernel = kernel;
		}

		public IDependencyScope BeginScope()
		{
			return new NinjectScope(_kernel.BeginBlock());
		}

		protected override void Dispose(bool disposing)
		{
			if (_kernel != null)
			{
				_kernel.Dispose();
				GC.Collect();
			}
			base.Dispose(disposing);
		}
	}
}