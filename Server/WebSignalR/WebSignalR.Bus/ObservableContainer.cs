using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Bus
{
	public class ObservableContainer<T> : IObservable<T> where T : class
	{
		private List<IObserver<T>> _observers;

		public ObservableContainer()
		{
			_observers = new List<IObserver<T>>();
		}

		public virtual IDisposable Subscribe(IObserver<T> observer)
		{
			Ensure.Argument.NotNull(observer, "observer");
			if (!_observers.Contains(observer))
				_observers.Add(observer);

			return new Unsubscribe(_observers, observer);
		}

		public virtual void OnNext(T value)
		{
			foreach (var item in _observers)
			{
				item.OnNext(value);
			}
		}

		private class Unsubscribe : IDisposable
		{
			private readonly List<IObserver<T>> _observers;
			private readonly IObserver<T> _curentObserver;

			public Unsubscribe(List<IObserver<T>> observers, IObserver<T> observer)
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

	public class CommonObserver<T> : IObserver<T> where T : class
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
		private readonly ConcurrentDictionary<Type, ObservableContainer<object>> _container;

		public MessagePipeline()
		{
			_container = new ConcurrentDictionary<Type, ObservableContainer<object>>();
		}

		public IDisposable Subscribe<T>(IObserver<T> observer) where T : class
		{
			Ensure.Argument.NotNull(observer, "observer");

			ObservableContainer<object> observable;
			if (_container.TryGetValue(typeof(T), out observable))
			{
				return observable.Subscribe((observer as IObserver<object>));
			}
			else
			{
				observable = new ObservableContainer<object>();
				IDisposable disp = observable.Subscribe((observer as IObserver<object>));
				_container.TryAdd(typeof(T), observable);
				return disp;
			}
		}

		public void Publish<T>(T value) where T : class
		{
			ObservableContainer<object> observable;
			if (_container.TryGetValue(typeof(T), out observable))
			{
				observable.OnNext(value);
			}
		}

		public void Dispose()
		{
			_container.Clear();
		}
	}
}
