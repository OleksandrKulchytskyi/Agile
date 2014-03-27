using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Entities
{
	public class EntityBase : IEntity<int>
	{
		[Key]
		[Required]
		public int Id { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropChanged(string name)
		{
			PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(name));
		}

		public event PropertyChangingEventHandler PropertyChanging;

		protected void OnPropChanging(string name)
		{
			PropertyChangingEventHandler handler = Interlocked.CompareExchange(ref PropertyChanging, null, null);
			if (handler != null)
				handler(this, new PropertyChangingEventArgs(name));
		}
	}
}