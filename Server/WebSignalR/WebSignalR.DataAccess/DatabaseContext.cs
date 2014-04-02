using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.DataAccess.DB
{
	public class DatabaseContext : DbContext, IContext
	{
		public DatabaseContext(string connection)
			: base(connection)
		{
		}

		public DatabaseContext(ICrypto crypto)
			: base(crypto.Decrypt(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionSettings"].ConnectionString))
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var typesToRegister = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
								.Where(type => !String.IsNullOrEmpty(type.Namespace) && type.Namespace.EndsWith("Mappings")
												&& type.BaseType.GetGenericTypeDefinition() == typeof(Mappings.MapBase<>));

			foreach (var type in typesToRegister)
			{
				dynamic configurationInstance = Activator.CreateInstance(type);
				modelBuilder.Configurations.Add(configurationInstance);
			}
			//Database.SetInitializer(new MigrateDatabaseToLatestVersion<DatabaseContext, Migrations.Configuration>());
			base.OnModelCreating(modelBuilder);
		}

		public bool EnableAuditLog { get; set; }

		public int Commit()
		{
			int result = SaveChanges();
			return result;
		}

		public void RollbackChanges()
		{
			var changedEntries = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();
			foreach (var entry in changedEntries)
			{
				switch (entry.State)
				{
					// Under the covers, changing the state of an entity from Modified to Unchanged first sets the values of all  
					// properties to the original values that were read from the database when it was queried, and then marks the  
					// entity as Unchanged. This will also reject changes to FK relationships since the original value of the FK will be restored. 
					case EntityState.Modified:
						entry.State = EntityState.Unchanged;
						break;
					case EntityState.Added:
						entry.State = EntityState.Detached;
						break;
					// If the EntityState is the Deleted, reload the date from the database.   
					case EntityState.Deleted:
						entry.Reload();
						break;
					default: break;
				}
			}

			//foreach (var entry in changedEntries.Where(x => x.State == EntityState.Modified))
			//{
			//	entry.CurrentValues.SetValues(entry.OriginalValues);
			//	entry.State = EntityState.Unchanged;
			//}

			//foreach (var entry in changedEntries.Where(x => x.State == EntityState.Added))
			//{
			//	entry.State = EntityState.Detached;
			//}

			//foreach (var entry in changedEntries.Where(x => x.State == EntityState.Deleted))
			//{
			//	entry.State = EntityState.Unchanged;
			//}
		}

		public System.Data.Common.DbTransaction BeginTransaction()
		{
			DbConnection connection = this.Database.Connection;
			if (connection.State != ConnectionState.Open && connection.State != ConnectionState.Connecting)
				connection.Open();

			return connection.BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public bool IsAuditEnabled { get; set; }

		public IDbSet<T> GetEntitySet<T>() where T : Common.Entities.EntityBase
		{
			return Set<T>();
		}

		public void ChangeState<T>(T entity, EntityState state) where T : Common.Entities.EntityBase
		{
			Entry<T>(entity).State = state;
		}

		private static bool IsChanged(DbEntityEntry entity)
		{
			return IsStateEqual(entity, EntityState.Added) ||
				   IsStateEqual(entity, EntityState.Deleted) ||
				   IsStateEqual(entity, EntityState.Modified);
		}

		private static bool IsStateEqual(DbEntityEntry entity, EntityState state)
		{
			return (entity.State & state) == state;
		}
	}
}