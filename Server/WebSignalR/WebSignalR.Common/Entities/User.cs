using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Entities
{
	public class User : IEntity<int>
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Password { get; set; }

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
	}
}