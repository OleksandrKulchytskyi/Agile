namespace WebSignalR.DataAccess.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class InitialVersion : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.Rooms",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						Name = c.String(nullable: false),
						Description = c.String(),
						Active = c.Boolean(nullable: false),
					})
				.PrimaryKey(t => t.Id);

			CreateTable(
				"dbo.Users",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						Name = c.String(nullable: false, maxLength: 50),
						Password = c.String(nullable: false),
						IsAdmin = c.Boolean(nullable: false),
						Room_Id = c.Int(),
					})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Rooms", t => t.Room_Id)
				.Index(t => t.Room_Id);

			CreateTable(
				"dbo.UserSessions",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						SessionId = c.String(nullable: false),
						LastActivity = c.DateTimeOffset(precision: 7),
						UserAgent = c.String(),
						UserId = c.Int(nullable: false),
					})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
				.Index(t => t.UserId);

			CreateTable(
				"dbo.Priviliges",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						Name = c.String(nullable: false),
						Description = c.String(),
					})
				.PrimaryKey(t => t.Id);

			CreateTable(
				"dbo.UserVotes",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						UserId = c.Int(nullable: false),
						VoteId = c.Int(nullable: false),
						Mark = c.Int(nullable: false),
					})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
				.ForeignKey("dbo.VoteItems", t => t.VoteId, cascadeDelete: true)
				.Index(t => t.UserId)
				.Index(t => t.VoteId);

			CreateTable(
				"dbo.VoteItems",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						Content = c.String(nullable: false),
						OverallMark = c.Int(nullable: false),
						RoomId = c.Int(nullable: false),
					})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
				.Index(t => t.RoomId);

			CreateTable(
				"dbo.UserPrivilege",
				c => new
					{
						PrivilegeId = c.Int(nullable: false),
						UserId = c.Int(nullable: false),
					})
				.PrimaryKey(t => new { t.PrivilegeId, t.UserId })
				.ForeignKey("dbo.Priviliges", t => t.PrivilegeId, cascadeDelete: true)
				.ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
				.Index(t => t.PrivilegeId)
				.Index(t => t.UserId);

		}

		public override void Down()
		{
			DropForeignKey("dbo.Users", "Room_Id", "dbo.Rooms");
			DropForeignKey("dbo.UserVotes", "VoteId", "dbo.VoteItems");
			DropForeignKey("dbo.VoteItems", "RoomId", "dbo.Rooms");
			DropForeignKey("dbo.UserVotes", "UserId", "dbo.Users");
			DropForeignKey("dbo.UserPrivilege", "UserId", "dbo.Users");
			DropForeignKey("dbo.UserPrivilege", "PrivilegeId", "dbo.Priviliges");
			DropForeignKey("dbo.UserSessions", "UserId", "dbo.Users");
			DropIndex("dbo.Users", new[] { "Room_Id" });
			DropIndex("dbo.UserVotes", new[] { "VoteId" });
			DropIndex("dbo.VoteItems", new[] { "RoomId" });
			DropIndex("dbo.UserVotes", new[] { "UserId" });
			DropIndex("dbo.UserPrivilege", new[] { "UserId" });
			DropIndex("dbo.UserPrivilege", new[] { "PrivilegeId" });
			DropIndex("dbo.UserSessions", new[] { "UserId" });
			DropTable("dbo.UserPrivilege");
			DropTable("dbo.VoteItems");
			DropTable("dbo.UserVotes");
			DropTable("dbo.Priviliges");
			DropTable("dbo.UserSessions");
			DropTable("dbo.Users");
			DropTable("dbo.Rooms");
		}
	}
}
