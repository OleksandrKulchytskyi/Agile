using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class SessionRoomMap : MapBase<SessionRoom>
	{
		public SessionRoomMap()
		{
			this.ToTable("SessionRooms");
			//this.Property(u => u.Id).HasColumnName("Id");
			this.Property(u => u.RoomName).HasColumnName("RoomName").IsRequired();
			this.Property(u => u.LoggedIn).HasColumnName("LoggedIn").IsRequired();
			this.Property(x => x.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
			//this.HasRequired(sr => sr.Session).WithOptional(x => x.SessionRoom);
		}
	}
}
