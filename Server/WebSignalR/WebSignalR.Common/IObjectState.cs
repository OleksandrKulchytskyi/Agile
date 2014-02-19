using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common
{
	public interface IObjectState
	{
		State State { get; set; }
	}

	public enum State
	{
		Added,
		Modified,
		Deleted,
		Unchanged
	}
}