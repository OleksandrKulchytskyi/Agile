﻿using WebSignalR.Common.Entities;

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
			//this.HasRequired(x => x.User).WithMany(x => x.ConnectedSessions).HasForeignKey(k => k.UserId).WillCascadeOnDelete();

			this.HasOptional(us => us.SessionRoom).WithRequired(sr => sr.Session).WillCascadeOnDelete();
		}
	}
}