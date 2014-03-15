using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class PrivilegesMap : MapBase<Privileges>
	{
		public PrivilegesMap()
			: base()
		{
			this.ToTable("Priviliges");

			this.Property(x => x.Name).HasColumnName("Name").IsRequired();
			this.Property(x => x.Description).HasColumnName("Description").IsOptional();

			this.HasMany(x => x.Users).WithMany(u => u.UserPrivileges).Map(up =>
			{
				up.MapLeftKey("PrivilegeId");
				up.MapRightKey("UserId");
				up.ToTable("UserPrivilege");
			});
		}
	}
}