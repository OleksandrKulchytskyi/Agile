using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using WebSignalR.Common.Interfaces;
using WebSignalR.DataAccess.Mappings;

namespace WebSignalR.DataAccess.DB
{
	public class DatabaseContext : DbContext, IContext
	{
		public DatabaseContext()
			: base("ConnectionSettings")
		{
		}

		public DatabaseContext(string connection)
			: base(connection)
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			//var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
			//					.Where(type => !String.IsNullOrEmpty(type.Namespace))
			//					.Where(type => type.BaseType != null && !type.BaseType.IsGenericType &&
			//									type.BaseType.GetGenericTypeDefinition() == typeof(Mappings.MapBase<>));
			//foreach (var type in typesToRegister)
			//{
			//	dynamic configurationInstance = Activator.CreateInstance(type);
			//	modelBuilder.Configurations.Add(configurationInstance);
			//}

			modelBuilder.Configurations.Add(new RoomMap());
			modelBuilder.Configurations.Add(new UserMap());
			modelBuilder.Configurations.Add(new UserSessionMap());
			modelBuilder.Configurations.Add(new VoteItemMap());
			modelBuilder.Configurations.Add(new UserVoteMap());
			modelBuilder.Configurations.Add(new PrivilegesMap());
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
			//TODO: add implemetations here
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