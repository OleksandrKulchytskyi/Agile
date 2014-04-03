using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WebSignalR.Bus;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces.Bus;

namespace WebSignalR.Test
{
	[TestClass]
	public class BusUnitTest
	{
		private volatile bool active = true;
		private IBus bus = null;

		[TestMethod]
		public void TestMethod1()
		{
			TaskHelper.Delay(TimeSpan.FromSeconds(15)).ContinueWith(prev =>
			{
				active = false;
				Debug.WriteLine("!!!!!!!!!!!!!  15 seconds were ticked  !!!!!!!!!!!!!!!!!!");
			});
			Task runTask = Task.Factory.StartNew(() => Run());
			runTask.Wait();

			TaskHelper.Delay(TimeSpan.FromSeconds(1)).Wait();

			Assert.IsTrue(bus.SubscriptionsCount == 0);
		}

		private void Run()
		{
			bus = new InMemoryBus();
			// Delegate Handler
			var stringSubsc = bus.Subscribe<string>(message => Debug.WriteLine("Delegate Handler Received: {0}", message));
			var stringSubsc2 = bus.Subscribe<string>((message, token) => WriteMessageAsync(message, token));

			// Strongly typed handler
			var msgSubscr = bus.Subscribe<Message>(() => new MessageHandler());
			// Strongly typed async handler
			var msgSubscr2 = bus.Subscribe<Message>(() => new AsyncMessageHandler()); // will automatically be passed a cancellation token

			Debug.WriteLine("Enter a message\n");

			string input = DateTime.UtcNow.ToString();
			while (active)
			{
				var t2 = bus.SendAsync(input);
				var t1 = bus.SendAsync(new Message { Body = input });

				Task.WaitAll(t1, t2);

				Debug.WriteLine("\nEnter another message\n");
				Thread.Sleep(1450);
			}

			bus.Unsubscribe(stringSubsc);
			bus.Unsubscribe(stringSubsc2);
			bus.Unsubscribe(msgSubscr);
			bus.Unsubscribe(msgSubscr2);
			bus.SendAsync("Hello");
		}

		private Task WriteMessageAsync(string message, CancellationToken cancellationToken)
		{
			return TaskHelper.Delay(2000).ContinueWith(task => Debug.WriteLine("Delegate Async Handler Received: {0}", message));
		}
	}

	public class Message
	{
		public string Body { get; set; }
	}

	public class MessageHandler : IHandle<Message>
	{
		public void Handle(Message message)
		{
			Debug.WriteLine("{0} Received sync message type: {1}", this.GetType().Name, typeof(Message).Name);
		}
	}

	public class AsyncMessageHandler : IHandleAsync<Message>
	{
		public Task HandleAsync(Message message, CancellationToken cancellationToken)
		{
			Task t = TaskHelper.Delay(1000);
			Debug.WriteLine("{0} Received async message type: {1}", this.GetType().Name, typeof(Message).Name);
			return t;
		}
	}
}
