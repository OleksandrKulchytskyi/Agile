using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSignalR.Common.Interfaces
{
	public interface ICsvProvider
	{
		void Init();

		bool IsReady(Guid id);
	}
}
