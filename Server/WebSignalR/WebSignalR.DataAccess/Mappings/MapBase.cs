using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using WebSignalR.Common.Entities;

namespace WebSignalR.DataAccess.Mappings
{
	public class MapBase<T> : EntityTypeConfiguration<T> where T : EntityBase
	{
		public MapBase()
		{
			// Primary Key
			this.HasKey(u => u.Id);
			this.Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity).IsRequired();
		}
	}
}