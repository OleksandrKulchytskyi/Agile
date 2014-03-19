using System.Threading.Tasks;

namespace WebSignalR.Common.Services
{
	public interface IRoomService
	{
		Task JoinRoom(string room, string sessionId);

		Task LeaveRoom(string room, string sessionId);

		Task ChangeRoomState(string room, bool state);
	}
}