using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebSignalR.Common
{
	//public override Task OnConnected()
	//{
	//	// NOT WORKING
	//	Debug.Print(MethodHandler.GetValue<int>(key => Clients(Context.ConnectionId).Client.GetValue(key)));
	
	//	// WORKING
	//	new Thread(() => Debug.Print(MethodHandler.GetValue<int>(key => Clients(Context.ConnectionId).Client.GetValue(key)))).Start();
	
	//	return base.OnConnected();
	//}

	public static class MethodHandler
	{
		private static ConcurrentDictionary<Guid, ReturnWaiter> runningMethodWaiters = new ConcurrentDictionary<Guid, ReturnWaiter>();

		public static TResult GetValue<TResult>(Action<Guid> requestValue)
		{
			Guid key = Guid.NewGuid();
			ReturnWaiter returnWaiter = new ReturnWaiter(key);
			runningMethodWaiters.TryAdd(key, returnWaiter);
			requestValue.Invoke(key);
			returnWaiter.Signal.WaitOne();
			return (TResult)returnWaiter.Value;
		}

		public static void GetValueResult(Guid key, object value)
		{
			ReturnWaiter waiter;
			if (runningMethodWaiters.TryRemove(key, out waiter))
			{
				waiter.Value = value;
			}
		}
	}

	internal class ReturnWaiter
	{
		private ManualResetEvent _signal = new ManualResetEvent(false);
		public ManualResetEvent Signal { get { return _signal; } }
		public Guid Key { get; private set; }

		public ReturnWaiter(Guid key)
		{
			Key = key;
		}

		private object _value;
		public object Value
		{
			get { return _value; }
			set
			{
				_value = value;
				Signal.Set();
			}
		}
	}
}