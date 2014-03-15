using System.Data.Entity;
using WebSignalR.Common;

namespace WebSignalR.DataAccess
{
	public static class EntityStateHelper
	{
		public static System.Data.Entity.EntityState ConvertState(this State entStata)
		{
			switch (entStata)
			{
				case State.Added: return System.Data.Entity.EntityState.Added;
				case State.Deleted: return System.Data.Entity.EntityState.Deleted;
				case State.Modified: return System.Data.Entity.EntityState.Modified;

				default:
					return System.Data.Entity.EntityState.Unchanged;
			}
		}

		//Only use with short lived DbContext!!!
		public static void ApplyStateChanges(this DbContext context)
		{
			foreach (var entry in context.ChangeTracker.Entries<IObjectState>())
			{
				IObjectState stateInfo = entry.Entity;
				if (stateInfo != null)
					entry.State = stateInfo.State.ConvertState();
			}
		}
	}
}