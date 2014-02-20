using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebSignalR.Common.Infrastructure;

namespace WebSignalR.Common.Entities
{
	public class Room : EntityBase
	{
		public Room()
		{
			ConnectedUsers = new SafeCollection<User>();
			ItemsToVote = new SafeCollection<VoteItem>();
		}

		private string name;
		[Required]
		public string Name
		{
			get { return name; }
			set { OnPropChanging("Name"); name = value; OnPropChanged("Name"); }
		}

		private string descr;
		public string Description
		{
			get { return descr; }
			set { OnPropChanging("Description"); descr = value; OnPropChanged("Description"); }
		}

		private bool active;
		[Required]
		public bool Active
		{
			get { return active; }
			set { OnPropChanging("Active"); active = value; OnPropChanged("Active"); }
		}

		public virtual ICollection<User> ConnectedUsers { get; set; }
		public virtual ICollection<VoteItem> ItemsToVote { get; set; }
	}
}