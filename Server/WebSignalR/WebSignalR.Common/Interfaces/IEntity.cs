using System.ComponentModel;

namespace WebSignalR.Common.Interfaces
{
	public interface IEntity<T> : INotifyPropertyChanged, INotifyPropertyChanging
	{
		T Id { get; set; }
	}
}