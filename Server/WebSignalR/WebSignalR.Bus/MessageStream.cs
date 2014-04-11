using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Bus
{
	public class MessageStream<T> : IObservable<T> where T : IBroadcastMessage
	{
		private List<IObserver<T>> _observers;

		public MessageStream()
		{
			_observers = new List<IObserver<T>>();
		}

		public virtual IDisposable Subscribe(IObserver<T> observer)
		{
			Ensure.Argument.NotNull(observer, "observer");
			if (!_observers.Contains(observer))
				_observers.Add(observer);

			return new StreamUnsubscriber(_observers, observer);
		}

		public virtual void Broadcast(T value)
		{
			foreach (var item in _observers)
			{
				item.OnNext(value);
			}
		}

		public void EndTransmission()
		{
			foreach (var observer in _observers.ToArray())
				if (_observers.Contains(observer))
					observer.OnCompleted();

			_observers.Clear();
		}

		private class StreamUnsubscriber : IDisposable
		{
			private readonly List<IObserver<T>> _observers;
			private readonly IObserver<T> _curentObserver;

			public StreamUnsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
			{
				Ensure.Argument.NotNull(observer, "observer");
				Ensure.Argument.NotNull(observers, "observers");

				_observers = observers;
				_curentObserver = observer;
			}

			public void Dispose()
			{
				if (_observers != null && _observers.Contains(_curentObserver))
					_observers.Remove(_curentObserver);
			}
		}
	}

	public class CommonObserver<T> : IObserver<T> where T : IBroadcastMessage
	{
		public T LastValue
		{
			get;
			private set;
		}

		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public virtual void OnNext(T value)
		{
			LastValue = value;
		}
	}

	public class MessagePipeline : IMessgaePipeline
	{
		private readonly ConcurrentDictionary<Type, MessageStream<IBroadcastMessage>> _container;

		public MessagePipeline()
		{
			_container = new ConcurrentDictionary<Type, MessageStream<IBroadcastMessage>>();
		}

		public IDisposable Subscribe<T>(IObserver<T> observer) where T : IBroadcastMessage
		{
			Ensure.Argument.NotNull(observer, "observer");

			MessageStream<IBroadcastMessage> observable;
			if (_container.TryGetValue(typeof(T), out observable))
			{
				return observable.Subscribe((observer as IObserver<IBroadcastMessage>));
			}
			else
			{
				observable = new MessageStream<IBroadcastMessage>();
				IDisposable disp = observable.Subscribe((observer as IObserver<IBroadcastMessage>));
				_container.TryAdd(typeof(T), observable);
				return disp;
			}
		}

		public void Publish<T>(T value) where T : IBroadcastMessage
		{
			Ensure.Argument.NotNull(value, "value");

			MessageStream<IBroadcastMessage> observable;
			if (_container.TryGetValue(typeof(T), out observable))
			{
				observable.Broadcast(value);
			}
		}

		public void Dispose()
		{
			_container.Clear();
		}
	}
}
