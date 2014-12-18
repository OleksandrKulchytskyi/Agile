using System;
using System.Linq;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Interfaces.Bus;
using WebSignalR.Common.Messages;

namespace WebSignalR.Infrastructure.Services
{
	public class CsvProvider : ICsvProvider
	{
		private readonly IBus _msgPipeline;
		private readonly IUnityOfWork _unity;
		private readonly ICsvStatePusher _pusher;
		private readonly IPurge cleaner;
		private string _appDataPath;
		private readonly string extension = ".csv";


		public CsvProvider(IBus bus, IUnityOfWork unity, ICsvStatePusher pusher, IPurge purge)
		{
			Ensure.Argument.NotNull(bus, "bus");
			Ensure.Argument.NotNull(unity, "unity");
			Ensure.Argument.NotNull(pusher, "pusher");
			Ensure.Argument.NotNull(purge, "purge");

			_msgPipeline = bus;
			_unity = unity;
			_pusher = pusher;
			cleaner = purge;
		}

		public void Init()
		{
			_appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
			_msgPipeline.Subscribe<ProvideCsvMessage>(HandleMessage);
		}

		private void HandleMessage(Common.Messages.ProvideCsvMessage message)
		{
			if (message != null)
			{
				TaskHelper.FromAction<ProvideCsvMessage>(provideCsvInternal, message).
					ContinueWithEx(t => { if (t.IsFaulted) Global.Logger.Error(t.Exception); });
			}
		}

		private void provideCsvInternal(ProvideCsvMessage message)
		{
			CsvStateChanged state = new CsvStateChanged();

			switch (message.State)
			{
				case CsvReadyState.Init:
					TaskHelper.Delay(TimeSpan.FromSeconds(1)).Wait();
					message.OutputId = Guid.NewGuid();
					message.State = Common.Messages.CsvReadyState.Processing;
					_msgPipeline.SendAsync(message);
					state.State = CsvReadyState.Init.ToString();
					break;

				case CsvReadyState.Processing:
					TaskHelper.Delay(TimeSpan.FromSeconds(1)).Wait();
					if (_appDataPath.IsNotNullOrEmpty())
					{
						state.State = CsvReadyState.Processing.ToString();
						message.State = Common.Messages.CsvReadyState.Collecting;
						string fpath = System.IO.Path.Combine(_appDataPath, message.OutputId.ToString("N")) + extension;
						using (var stream = System.IO.File.Create(fpath)) { }
						_msgPipeline.SendAsync(message);
					}
					break;

				case CsvReadyState.Collecting:
					TaskHelper.Delay(TimeSpan.FromSeconds(1)).Wait();
					state.State = CsvReadyState.Collecting.ToString();
					IReadOnlyRepository<Room> roomPepo = GetRepository<Room>();
					Room room = roomPepo.Get(x => x.Id == message.RoomId).FirstOrDefault();
					System.Text.StringBuilder sb = null;
					if (room != null)
					{
						sb = new System.Text.StringBuilder();
						sb.AppendLine("VoteItem, Voted users");
						foreach (var item in room.ItemsToVote)
						{
							sb.Append(item.Content);
							foreach (var subItem in item.VotedUsers.OrderBy(x => x.UserId))
							{
								sb.Append("," + subItem.UserId + "_" + subItem.Mark);
							}
							sb.Append(Environment.NewLine);
						}
					}
					if (_appDataPath.IsNotNullOrEmpty())
					{
						string fpath = System.IO.Path.Combine(_appDataPath, message.OutputId.ToString("N") + extension);
						using (var stream = System.IO.File.OpenWrite(fpath))
						{
							using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
							using (var sr = new System.IO.StringReader(sb.ToString()))
							{
								string line;
								while ((line = sr.ReadLine()) != null)
								{
									sw.WriteLine(line);
								}
								sw.Flush();
							}
						}
					}
					message.State = Common.Messages.CsvReadyState.Ready;
					_msgPipeline.SendAsync(message);
					break;

				case CsvReadyState.Ready:
					TaskHelper.Delay(TimeSpan.FromSeconds(1)).Wait();
					state.FileId = message.OutputId.ToString("N");
					state.State = CsvReadyState.Ready.ToString();
					break;

				default:
					break;

			}
			if (state != null)
				_pusher.Notify(state);
		}

		public bool IsReady(Guid id)
		{
			if (!string.IsNullOrEmpty(_appDataPath))
			{
				var files = System.IO.Directory.GetFiles(_appDataPath, "*" + extension).Where(x => x.IndexOf(id.ToString("N"),
					StringComparison.OrdinalIgnoreCase) != -1).ToList();
				if (files.Count > 0)
				{
					bool result = true;
					System.IO.FileStream fs = null;
					try
					{
						fs = System.IO.File.OpenRead(files[1]);
					}
					catch (System.IO.IOException)
					{
						result = false;
					}
					finally { if (fs != null) fs.Dispose(); }
					return result;
				}
			}
			return false;
		}

		public void Purge(Guid id)
		{
			cleaner.AddPurgeTask(id);
		}

		private class CsvStateChanged : IBroadcastMessage
		{
			public string FileId { get; set; }
			public string State { get; set; }
		}

		private IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase
		{
			return _unity.GetRepository<TEntity>();
		}
	}
}