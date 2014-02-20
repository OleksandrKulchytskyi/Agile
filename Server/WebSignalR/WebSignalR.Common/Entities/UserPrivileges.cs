
namespace WebSignalR.Common.Entities
{
	public class UserPrivileges : EntityBase
	{
		private int userId;
		public int UserId
		{
			get { return userId; }
			set { userId = value; }
		}

		public virtual User User { get; set; }

		private int privId;
		public int PrivilegeId
		{
			get { return privId; }
			set { privId = value; }
		}

		public virtual Privileges Privilege { get; set; }
	}
}