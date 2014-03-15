namespace WebSignalR.DataAccess.Migrations
{
	using System.Data.Entity.Migrations;

	public partial class VoteItem_Closed : DbMigration
	{
		public override void Up()
		{
			AddColumn("dbo.VoteItems", "Closed", c => c.Boolean(nullable: false));
		}

		public override void Down()
		{
			DropColumn("dbo.VoteItems", "Closed");
		}
	}
}