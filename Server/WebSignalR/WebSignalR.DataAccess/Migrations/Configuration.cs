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
				Privileges userPriv = new Privileges { Name = "User", Description = "Default system user" };
				context.Set<Privileges>().Add(userPriv);
				context.Set<Room>().Add(new Room() { Active = true, Description = "DEBUG room for testing purposes", Name = "TestRoom" });
				context.Set<Room>().Add(new Room() { Active = false, Description = "Project room for testing purposes", Name = "Project" });

				usrAdmin.UserPrivileges.Add(userPriv);

				context.SaveChanges();

				Privileges usrP = context.Set<Privileges>().FirstOrDefault(x => x.Name == "User");
				if (usrP != null)
				{
					User usr1 = new User();
					usr1.IsAdmin = false;
					usr1.Name = "User1";
					usr1.Password = "user1".toBase64Utf8();
					usr1.UserPrivileges.Add(usrP);

					User usr2 = new User();
					usr2.IsAdmin = false;
					usr2.Name = "User2";
					usr2.Password = "user2".toBase64Utf8();
					usr2.UserPrivileges.Add(usrP);

					context.Set<User>().Add(usr1);
					context.Set<User>().Add(usr2);

					context.SaveChanges();
				}
			}
		}
	}
}
