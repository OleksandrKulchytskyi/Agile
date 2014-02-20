using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class VoteItemMap : MapBase<VoteItem>
	{
		public VoteItemMap()
			: base()
		{
			this.ToTable("VoteItems");

			this.Property(v => v.Content).HasColumnName("Content").IsRequired();
			this.Property(v => v.OverallMark).HasColumnName("OverallMark");

			this.HasRequired(x => x.HostRoom).WithMany(x => x.ItemsToVote).HasForeignKey(x => x.RoomId);
		}

	}
}
