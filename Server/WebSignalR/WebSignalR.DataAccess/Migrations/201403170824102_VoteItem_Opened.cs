namespace WebSignalR.DataAccess.Migrations
{
	using System.Data.Entity.Migrations;

	public partial class VoteItem_Opened : DbMigration
	{
		public override void Up()
		{
			AddColumn("dbo.VoteItems", "Opened", c => c.Boolean(nullable: false));
		}

		public override void Down()
		{
			DropColumn("dbo.VoteItems", "Opened");
		}
	}
}