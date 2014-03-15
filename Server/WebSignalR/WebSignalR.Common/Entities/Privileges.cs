using System.Collections.Generic;
using WebSignalR.Common.Infrastructure;

namespace WebSignalR.Common.Entities
{
	public class Privileges : EntityBase
	{
		public Privileges()
		{
			Users = new SafeCollection<User>();
		}

		private string name;

		public string Name
		{
			get { return name; }
			set { OnPropChanging("Name"); name = value; OnPropChanged("Name"); }
		}

		private string description;

		public string Description
		{
			get { return description; }
			set { description = value; OnPropChanged("Description"); }
		}

		public ICollection<User> Users { get; set; }
	}
}