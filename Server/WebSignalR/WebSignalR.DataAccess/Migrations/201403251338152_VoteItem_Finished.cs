namespace WebSignalR.DataAccess.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class VoteItem_Finished : DbMigration
	{
		public override void Up()
		{
			AddColumn("dbo.VoteItems", "Finished", c => c.Boolean(nullable: false));
		}

		public override void Down()
		{
			DropColumn("dbo.VoteItems", "Finished");
		}
	}
}
