using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSignalR.Common.Interfaces.Bus
{
	public interface IBus
	{
		Task SendAsync<TMessage>(TMessage message);
		Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
		Guid Subscribe<TMessage>(Action<TMessage> handler);
		Guid Subscribe<TMessage>(Func<TMessage, CancellationToken, Task> handler);
		void Unsubscribe(Guid subscriptionId);

		int SubscriptionsCount { get; }
	}
}