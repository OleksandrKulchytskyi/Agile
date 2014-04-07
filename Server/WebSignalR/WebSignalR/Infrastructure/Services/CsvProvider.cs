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
		private string _appDataPath;

		public CsvProvider(IBus bus, IUnityOfWork unity, ICsvStatePusher pusher)
		{
			Ensure.Argument.NotNull(bus, "bus");
			Ensure.Argument.NotNull(unity, "unity");
			Ensure.Argument.NotNull(pusher, "pusher");
			
			_msgPipeline = bus;
			_unity = unity;
			_pusher = pusher;
		}

		public void Init()
		{
			_appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
			_msgPipeline.Subscribe<ProvideCsvMessage>(HandleMessage);
			_msgPipeline.Subscribe<TestMessage>(HandleTestMessage);
		}

		private void HandleTestMessage(TestMessage msg)
		{
			if (msg != null)
			{
				
			}
		}

		private void HandleMessage(Common.Messages.ProvideCsvMessage message)
		{
			if (message != null)
			{
				TaskHelper.FromAction<ProvideCsvMessage>(provideCsv, message).
					ContinueWithEx(t => { if (t.IsFaulted) Global.Logger.Error(t.Exception); });
			}
		}

		private void provideCsv(ProvideCsvMessage message)
		{
			CsvStateChanged state = new CsvStateChanged();

			switch (message.State)
			{
				case CsvReadyState.Init:
					message.OutputId = Guid.NewGuid();
					message.State = Common.Messages.CsvReadyState.Processing;
					_msgPipeline.SendAsync(message);
					state.State = CsvReadyState.Init.ToString();
					break;

				case CsvReadyState.Processing:
					if (_appDataPath.IsNotNullOrEmpty())
					{
						state.State = CsvReadyState.Processing.ToString();
						message.State = Common.Messages.CsvReadyState.Collecting;
						string fpath = System.IO.Path.Combine(_appDataPath, message.OutputId.ToString("N")) + ".csv";
						using (var stream = System.IO.File.Create(fpath)) { }
						_msgPipeline.SendAsync(message);
					}
					break;

				case CsvReadyState.Collecting:
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
						string fpath = System.IO.Path.Combine(_appDataPath, message.OutputId.ToString("N"));
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
					state.FileId = message.OutputId.ToString("N");
					state.State = CsvReadyState.Ready.ToString();
					break;

				default:
					break;

			}
			if (state != null)
				_pusher.Notify(state);
		}

		private class CsvStateChanged : IBroadcastMessage
		{
			public string FileId { get; set; }
			public string State { get; set; }
		}

		public bool IsReady(Guid id)
		{
			throw new NotImplementedException();
		}

		private IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase
		{
			return _unity.GetRepository<TEntity>();
		}
	}
}