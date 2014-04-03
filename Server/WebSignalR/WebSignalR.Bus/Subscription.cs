using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSignalR.Common.Extension;

namespace WebSignalR.Bus
{
	internal sealed class Subscription
	{
		public Guid Id { get; private set; }
		public Func<object, CancellationToken, Task> Handler { get; private set; }

		Subscription(Func<object, CancellationToken, Task> handler)
		{
			Ensure.Argument.NotNull(handler, "handler");
			Id = Guid.NewGuid();
			Handler = handler;
		}

		public static Subscription Create<TMessage>(Func<TMessage, CancellationToken, Task> handler)
		{
			Func<object, CancellationToken, Task> handlerWithCheck = (message, cancellationToken) =>
			{
				if (message.GetType().Implements<TMessage>())
				{
					return handler.Invoke((TMessage)message, cancellationToken);
				}
				return Task.Factory.StartNew(() => { });
			};

			return new Subscription(handlerWithCheck);
		}
	}
}
