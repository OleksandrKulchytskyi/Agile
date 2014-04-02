using WebSignalR.Common.Interfaces;
using WebSignalR.DataAccess.DB;

namespace WebSignalR.DataAccess.Initilaizers
{
	public class DatabaseContextInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<DatabaseContext>
	{
		public DatabaseContextInitializer(ICrypto crypto)
		{
			using (var dep = new DatabaseContext(crypto))
			{
				this.InitializeDatabase(dep);
				this.Seed(dep);
			}
		}

		protected override void Seed(DatabaseContext context)
		{
			if (context.Database.CompatibleWithModel(false) == true)
				context.Database.Initialize(true);
			//context.Database.Create();

			var DBScript = CreateDatabaseScript(context);
			if (!string.IsNullOrEmpty(DBScript))
			{
			}

			base.Seed(context);
		}

		public static string CreateDatabaseScript(System.Data.Entity.DbContext context)
		{
			return ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript();
		}
	}
}