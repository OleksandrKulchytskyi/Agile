using System;

namespace WebSignalR.Common.Interfaces
{
	public interface IMessgaePipeline : IDisposable
	{
		IDisposable Subscribe<T>(IObserver<T> observer) where T : class;

		void Publish<T>(T value) where T : class;
	}
}
