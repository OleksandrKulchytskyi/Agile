namespace WebSignalR.DataAccess.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using WebSignalR.Common.Entities;
	using WebSignalR.Common.Extension;

	public sealed class Configuration : DbMigrationsConfiguration<WebSignalR.DataAccess.DB.DatabaseContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
			//AutomaticMigrationDataLossAllowed = true;
		}

		protected override void Seed(WebSignalR.DataAccess.DB.DatabaseContext context)
		{
			if (!context.Set<User>().Any())
			{
				User usrAdmin = new User();
				usrAdmin.IsAdmin = true;
				usrAdmin.Name = "Admin";
				usrAdmin.Password = "admin1234".toBase64Utf8();
				usrAdmin.UserPrivileges.Add(new Privileges() { Name = "Admin", Description = "Administrator role." });
				usrAdmin.UserPrivileges.Add(new Privileges() { Name = "ScrumMaster", Description = "Scrum master role." });

				context.Set<User>().Add(usrAdmin);
				context.Set<Privileges>().Add(new Privileges { Name = "User", Description = "Default system user" });
				context.Set<Room>().Add(new Room() { Active = true, Description = "DEBUG room for testing purposes", Name = "TestRoom" });
				context.Set<Room>().Add(new Room() { Active = false, Description = "Project room for testing purposes", Name = "Project" });

				context.SaveChanges();
			}
		}
	}
}
