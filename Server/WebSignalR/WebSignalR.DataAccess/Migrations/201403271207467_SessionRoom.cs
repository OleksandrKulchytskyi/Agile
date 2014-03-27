namespace WebSignalR.DataAccess.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class SessionRoom : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.SessionRooms",
				c => new
					{
						Id = c.Int(nullable: false),
						RoomName = c.String(nullable: false),
						LoggedIn = c.DateTimeOffset(nullable: false, precision: 7),
					})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.UserSessions", t => t.Id, cascadeDelete: true)
				.Index(t => t.Id);

		}

		public override void Down()
		{
			DropForeignKey("dbo.SessionRooms", "Id", "dbo.UserSessions");
			DropIndex("dbo.SessionRooms", new[] { "Id" });
			DropTable("dbo.SessionRooms");
		}
	}
}
