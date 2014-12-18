namespace WebSignalR.Common.Entities
{
	public class UserVote : EntityBase
	{
		public int UserId { get; set; }

		public virtual User User { get; set; }

		public int VoteId { get; set; }

		public VoteItem VoteItem { get; set; }

		private int mark;

		public int Mark
		{
			get { return mark; }
			set { OnPropChanging("Mark"); mark = value; OnPropChanged("Mark"); }
		}
	}
}