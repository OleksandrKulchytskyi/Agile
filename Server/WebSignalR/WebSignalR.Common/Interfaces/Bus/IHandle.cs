using System.Threading;
using System.Threading.Tasks;

namespace WebSignalR.Common.Interfaces.Bus
{
	public interface IHandle<TMessage>
	{
		void Handle(TMessage message);
	}

	public interface IHandleAsync<TMessage>
	{
		Task HandleAsync(TMessage message, CancellationToken cancellationToken);
	}
}