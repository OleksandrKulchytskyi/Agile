using System.Threading.Tasks;

namespace WebSignalR.Common.Services
{
	public interface IRoomService
	{
		JoinRoomResult JoinRoom(string room, string sessionId);

		Task LeaveRoom(string room, string sessionId);

		Task ChangeRoomState(string room, bool state);
	}

	public class JoinRoomResult
	{
		public bool Active { get; set; }

		public bool HostMaster { get; set; }
	}
}