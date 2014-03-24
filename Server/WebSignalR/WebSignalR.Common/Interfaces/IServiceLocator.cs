using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IServiceLocator
	{
		IKernel Kernel { get; }

		void InitBindings(Action<IKernel> initializer);

		T Get<T>();
		T Get<T>(string named);
	}
}