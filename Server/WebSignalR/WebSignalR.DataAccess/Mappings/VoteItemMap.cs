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

			this.Property(v => v.Closed).HasColumnName("Closed").IsRequired();
			this.Property(v => v.Opened).HasColumnName("Opened").IsRequired();
			this.Property(v => v.Finished).HasColumnName("Finished").IsRequired();

			this.HasRequired(x => x.HostRoom).WithMany(x => x.ItemsToVote).HasForeignKey(x => x.RoomId);
		}
	}
}