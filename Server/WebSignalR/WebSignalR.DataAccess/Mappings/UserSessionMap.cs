using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class UserSessionMap : MapBase<UserSession>
	{
		public UserSessionMap()
			: base()
		{
			this.ToTable("UserSessions");
			this.Property(x => x.SessionId).HasColumnName("SessionId").IsRequired();
			this.Property(x => x.UserAgent).HasColumnName("UserAgent").IsOptional();
			this.Property(x => x.LastActivity).HasColumnName("LastActivity").IsOptional();

			this.HasRequired(x => x.User).WithMany(x => x.ConnectedSessions).HasForeignKey(k => k.UserId);
		}
	}
}
