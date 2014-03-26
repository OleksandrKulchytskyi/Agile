using Ninject;
using System;

namespace WebSignalR.Common.Interfaces
{
	public interface IServiceLocator
	{
		IKernel Kernel { get; }

		void InitBindings(Action<IKernel> initializer);
		void LoadModule(string path);

		T Get<T>();
		T Get<T>(string named);
	}
}