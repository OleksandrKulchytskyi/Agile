using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Entities
{
	public class User : EntityBase
	{
		public User()
		{
			ConnectedSession = new SafeCollection<UserSession>();
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

		public virtual ICollection<UserSession> ConnectedSession { get; set; }
	}
}