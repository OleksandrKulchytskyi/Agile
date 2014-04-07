using System;

namespace WebSignalR.Common.Interfaces
{
	public interface IBroadcastMessage { }

	public interface IMessgaePipeline : IDisposable
	{
		IDisposable Subscribe<T>(IObserver<T> observer) where T : IBroadcastMessage;

		void Publish<T>(T value) where T : IBroadcastMessage;
	}
}
