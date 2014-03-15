using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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

	public class XmlVotesProvider : IVotesProvider
	{
		public object Source { get; set; }

		public ICollection<VoteItem> GetVotes()
		{
			if (Source != null)
			{
				XDocument doc = XDocument.Load(Source as Stream);
				IEnumerable<VoteItem> data = doc.Root.Descendants("Vote").Select(x => new VoteItem() { Closed = false, Content = x.Value });
				return data.ToList();
			}

			return null;
		}
	}
}