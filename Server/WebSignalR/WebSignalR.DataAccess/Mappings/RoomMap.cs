using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class RoomMap : MapBase<Room>
	{
		public RoomMap()
			: base()
		{
			this.ToTable("Rooms");

			this.Property(m => m.Active).IsRequired();
			this.Property(x => x.Name).IsRequired();
			this.Property(x => x.Description).IsOptional();
		}
	}
}