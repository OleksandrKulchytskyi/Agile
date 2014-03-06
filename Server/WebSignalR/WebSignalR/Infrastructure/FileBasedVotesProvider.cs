using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure
{
	public class FileBasedVotesProvider : IVotesProvider
	{
		public object Source { get; set; }

		public ICollection<Common.Entities.VoteItem> GetVotes()
		{
			List<VoteItem> items = null;
			if (Source != null)
			{
				items = new List<VoteItem>();
				using (StreamReader sr = new StreamReader(Source as string))
				{
					string line = null;
					while ((line = sr.ReadLine()) != null)
					{
						items.Add(new VoteItem() { Closed = false, Content = line });
					}
				}
			}

			return items;
		}
	}
}