using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

		private bool active;
		[Required]
		//[DataMember(IsRequired = true)]
		public bool Active
		{
			get { return active; }
			set { OnPropChanging("Active"); active = value; OnPropChanged("Active"); }
		}

		private string name;
		[Required]
		//[DataMember(IsRequired = true)]
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

		public virtual ICollection<User> ConnectedUsers { get; set; }
		public virtual ICollection<VoteItem> ItemsToVote { get; set; }
	}
}