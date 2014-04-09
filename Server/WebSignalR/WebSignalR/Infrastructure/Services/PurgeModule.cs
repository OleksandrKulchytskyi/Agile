using System;
using System.Collections.Generic;
using System.Threading;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure.Services
{
	public class PurgeModule : IPurge
	{
		private readonly string extension = ".csv";
		private readonly string _appDataPath;

		private readonly Queue<Guid> guids;
		private readonly object sync;
		private volatile bool isStopped = false;
		private Thread thread;
		private int counter;

		public PurgeModule()
		{
			guids = new Queue<Guid>();
			sync = new object();
			counter = 0;
			_appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");

			thread = new Thread(Worker);
			thread.IsBackground = true;
			thread.Name = "Purge thread";
			thread.Start();
		}

		public void AddPurgeTask(Guid id)
		{
			lock (sync)
			{
				guids.Enqueue(id);
				Interlocked.Increment(ref counter);
				Monitor.PulseAll(sync);
			}
		}

		private void Worker()
		{
			while (!isStopped)
			{
				if (Interlocked.CompareExchange(ref counter, 0, 0) == 0)
					lock (sync)
						Monitor.Wait(sync);

				Guid id = guids.Dequeue();
				Interlocked.Decrement(ref counter);
				string fpath = System.IO.Path.Combine(_appDataPath, id.ToString("N") + extension);
				System.Threading.Tasks.Task.Factory.StartNew(Clean, fpath);
			}
		}

		private void Clean(object state)
		{
			WebSignalR.Common.Extension.TaskHelper.Delay(TimeSpan.FromSeconds(2)).Wait();
			string fpath = state as string;
			if (fpath != null)
			{
				try
				{
					if (System.IO.File.Exists(fpath))
						System.IO.File.Delete(fpath);
				}
				catch { }
			}
		}

		public void Dispose()
		{
			if (!isStopped)
			{
				isStopped = true;
			}
		}
	}
}