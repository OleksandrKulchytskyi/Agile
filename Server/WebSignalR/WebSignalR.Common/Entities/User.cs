using System.Collections.Generic;
using WebSignalR.Common.Infrastructure;

namespace WebSignalR.Common.Entities
{
	public class User : EntityBase
	{
		public User()
		{
			ConnectedSessions = new SafeCollection<UserSession>();
			UserPrivileges = new SafeCollection<Privileges>();
			UserVotes = new SafeCollection<UserVote>();
		}

		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				OnPropChanging("Name");
				name = value;
				OnPropChanged("Name");
			}
		}

		private string password;

		public string Password
		{
			get { return password; }
			set
			{
				OnPropChanging("Password");
				password = value;
				OnPropChanged("Password");
			}
		}

		private bool isAdmin;

		public bool IsAdmin
		{
			get { return isAdmin; }
			set { OnPropChanging("IsAdmin"); isAdmin = value; OnPropChanged("IsAdmin"); }
		}

		public virtual ICollection<UserSession> ConnectedSessions { get; set; }

		public virtual ICollection<Privileges> UserPrivileges { get; set; }

		public virtual ICollection<UserVote> UserVotes { get; set; }
	}
}