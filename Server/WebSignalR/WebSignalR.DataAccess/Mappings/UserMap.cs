using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class UserMap : MapBase<User>
	{
		public UserMap()
			: base()
		{
			// Table & Column Mappings
			this.ToTable("Users");
			this.Property(u => u.Id).HasColumnName("Id");
			this.Property(u => u.Name).HasColumnName("Name").IsRequired().HasMaxLength(50); ;
			this.Property(u => u.Password).HasColumnName("Password").IsRequired();

			//this.HasMany(x => x.ReadMessages).WithMany(x => x.UsersWhoRead)
			//.Map(m => m.MapLeftKey("User_Key").MapRightKey("Message_Key").ToTable("UserReadMessage"));
		}
	}
}
