using System;
using System.Collections.Generic;
using WebSignalR.Common.Entities;

namespace WebSignalR.Common.Services
{
	public interface ISessionService : IDisposable
	{
		IList<UserSession> GetSessions(int userId, bool inactive);

		IList<UserSession> GetSessions(string name, bool onlyInactive);

		UserSession GetCurrentSession(string user, string sessionId);

		void UpdateSessionActivity(string sessionId);

		void RemoveAllSessions();
	}
}