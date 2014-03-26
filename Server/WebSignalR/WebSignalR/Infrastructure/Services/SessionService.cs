using System;
using System.Collections.Generic;
using System.Linq;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;

namespace WebSignalR.Infrastructure.Services
{
	public sealed class SessionService : ISessionService
	{
		private readonly IUnityOfWork _unity;
		private readonly IRepository<UserSession> usRepository;

		public SessionService(IUnityOfWork unity)
		{
			_unity = unity;
			usRepository = _unity.GetRepository<UserSession>();
		}

		public IList<Common.Entities.UserSession> GetSessions(int userId, bool onlyInactive)
		{
			if (onlyInactive)
				return usRepository.Get(x => x.User.Id == userId && ElapsedTime(x.LastActivity) > 60).ToList();
			return usRepository.Get(x => x.User.Id == userId).ToList();
		}

		private int ElapsedTime(DateTimeOffset lastTiime)
		{
			return (int)(DateTime.UtcNow - lastTiime).TotalMinutes;
		}

		public System.Collections.Generic.IList<Common.Entities.UserSession> GetSessions(string name, bool onlyInactive)
		{
			if (onlyInactive)
				return usRepository.Get(x => x.User.Name == name && ElapsedTime(x.LastActivity) > 60).ToList();
			return _unity.GetRepository<UserSession>().Get(x => x.User.Name == name).ToList();
		}

		public void UpdateSessionActivity(string sessionId)
		{
			UserSession session = usRepository.Get(x => x.SessionId == sessionId).FirstOrDefault();
			if (session != null)
			{
				session.SetLastActivityNow();
				usRepository.Update(session);
				_unity.Commit();
			}
		}

		public UserSession GetCurrentSession(string user, string sessionId)
		{
			return usRepository.Get(x => x.SessionId == sessionId && x.User.Name == user).FirstOrDefault();
		}

		public void RemoveAllSessions()
		{
			var sessions = usRepository.GetAll();
			foreach (var session in sessions)
			{
				usRepository.Remove(session);
			}
			_unity.Commit();
		}

		public void Dispose()
		{
			if (_unity != null)
				_unity.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}