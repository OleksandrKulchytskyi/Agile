using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
