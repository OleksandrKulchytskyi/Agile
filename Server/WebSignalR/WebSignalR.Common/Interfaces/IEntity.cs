using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IEntity<T> : INotifyPropertyChanged, INotifyPropertyChanging
	{
		T Id { get; set; }
	} 
}