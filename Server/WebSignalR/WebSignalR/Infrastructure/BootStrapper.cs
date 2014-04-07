using AutoMapper;
using Microsoft.AspNet.SignalR;
using Ninject;
using System;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;
using WebSignalR.DataAccess.Repositories;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(WebSignalR.Infrastructure.BootStrapper), "PreAppStart")]
namespace WebSignalR.Infrastructure
{
	public static class BootStrapper
	{
		private static bool _sweeping;
		private static Timer _timerClaen;
		private static readonly TimeSpan _sweepInterval = TimeSpan.FromMinutes(29);
		private const string SqlClient = "System.Data.SqlClient";
		private static int purgeTime = ConfigurationManager.AppSettings["Sessions.PurgeTime"].ParseInt(55);

		internal static IServiceLocator serviceLocator;
		private static IDependencyResolver _signalrResolver = null;

		public static void PreAppStart()
		{
			serviceLocator = Services.ServiceLocator.Default;
			serviceLocator.InitBindings(IniDIContainer);

			_signalrResolver = new DependencyResolvers.NinjectSignalRDependencyResolver(serviceLocator.Kernel);
			App_Start.SignalRConfig.Register(_signalrResolver);

			System.Web.Mvc.DependencyResolver.SetResolver(new DependencyResolvers.NInjectMvcDependencyResolver(serviceLocator.Kernel));
			var unityFactory = new Func<IUnityOfWork>(() => serviceLocator.Kernel.Get<IUnityOfWork>());
			_timerClaen = new Timer(_ => Sweep(unityFactory, _signalrResolver), null, _sweepInterval, _sweepInterval);
		}

		private static void IniDIContainer(IKernel kernel)
		{
			kernel.Bind<System.Web.Mvc.IControllerActivator>().To<DependencyResolvers.NinjectMvcControllerActivator>();
			kernel.Bind<Microsoft.AspNet.SignalR.Hubs.IHubConnectionContext>().ToMethod(context =>
						_signalrResolver.Resolve<Microsoft.AspNet.SignalR.Infrastructure.IConnectionManager>().GetHubContext<Hubs.AgileHub>().Clients).
						Named("AgileHub");

			serviceLocator.LoadModule("~/Modules/BusModule.xml");
			serviceLocator.LoadModule("~/Modules/DataAccessModule.xml");

			kernel.Bind<IUnityOfWork>().ToMethod(context =>
			{
				IRoomRepository room = kernel.Get<IRoomRepository>();
				IUserRepository user = kernel.Get<IUserRepository>();
				ISessionRepository session = kernel.Get<ISessionRepository>();
				ISessionRoomRepository srRepo = kernel.Get<ISessionRoomRepository>();
				IPrivilegeRepository privileges = kernel.Get<IPrivilegeRepository>();
				IVoteItemRepository vote = kernel.Get<IVoteItemRepository>();
				IUserVoteRepository uv = kernel.Get<IUserVoteRepository>();
				IContext ctx = kernel.Get<IContext>();

				return new UnityOfWork(user, room, privileges, vote, session, uv, srRepo, ctx);
			});

			serviceLocator.LoadModule("~/Modules/ServicesModule.xml");


			WebSignalR.Common.Interfaces.Bus.IBus bus2 = serviceLocator.Get<WebSignalR.Common.Interfaces.Bus.IBus>();
			if (bus2 != null)
			{
				System.Diagnostics.Debug.WriteLine(bus2.Id);
			}
			ICsvProvider provider = serviceLocator.Get<ICsvProvider>();
			if (provider != null)
			{
				provider.Init();
			}

			kernel.Bind<Hubs.AgileHub>().ToMethod(context =>
			{
				IUnityOfWork unity = context.Kernel.Get<IUnityOfWork>();
				ICryptoService crypto = context.Kernel.Get<ICryptoService>();
				IUserRoomService userRoomSrv = context.Kernel.Get<IUserRoomService>();
				//ISessionService sessions = context.Kernel.Get<ISessionService>(); // In such way we reached exception ralated to the multiple instance of the IEntityChangeTracker. 
				ISessionService sessions = new Infrastructure.Services.SessionService(unity);
				Common.Interfaces.Bus.IBus bus = context.Kernel.Get<Common.Interfaces.Bus.IBus>();
				return new Hubs.AgileHub(unity, crypto, userRoomSrv, sessions, bus);
			});

			var service = new Func<ISessionService>(() => serviceLocator.Get<ISessionService>());
			ClearConnectedClients(service());
		}

		private static void ClearConnectedClients(ISessionService sessionServ)
		{
			if (sessionServ == null)
				return;
			try
			{
				sessionServ.RemoveAllSessions();
			}
			catch (Exception ex)
			{
				if (Global.Logger != null)
					Global.Logger.Error("ClearConnectedClients", ex);
			}
			finally
			{
				sessionServ.Dispose();
			}
		}

		private static void Sweep(Func<IUnityOfWork> unityFactory, IDependencyResolver resolver)
		{
			if (_sweeping) return;

			_sweeping = true;
			Global.Logger.InfoFormat("Begin sweep process");
			try
			{
				using (IUnityOfWork repo = unityFactory())
				{
					if (MarkInactiveSessions(repo.GetRepository<UserSession>(), resolver))
						repo.Commit();
				}
			}
			catch (Exception ex)
			{
				if (Global.Logger != null)
					Global.Logger.Error(ex);
			}
			finally
			{
				_sweeping = false;
			}
		}

		private static bool MarkInactiveSessions(IRepository<UserSession> repo, IDependencyResolver resolver)
		{
			var connectionManager = resolver.Resolve<Microsoft.AspNet.SignalR.Infrastructure.IConnectionManager>();
			var hubContext = connectionManager.GetHubContext<Hubs.AgileHub>();
			var inactiveSession = new System.Collections.Generic.List<UserSession>();

			System.Collections.Generic.IEnumerable<UserSession> sessions = repo.GetAll();

			foreach (var session in sessions)
			{
				var elapsed = DateTime.UtcNow - session.LastActivity;
				if (elapsed.TotalMinutes > purgeTime)
					inactiveSession.Add(session);
			}

			if (inactiveSession.Count > 0)
			{
				foreach (var inactive in inactiveSession)
				{
					repo.Remove(inactive);
				}
				return true;

				#region commented

				//var roomGroups = from usr in inactiveSession
				//				 from grp in usr.Groups
				//				 select new { User = usr, Group = grp } into tuple
				//				 group tuple by tuple.Group into g
				//				 select new
				//				 {
				//					 Group = g.Key,
				//					 Users = g.Select(t => new UserViewModel(t.User))
				//				 };

				//var parallelOpt = new System.Threading.Tasks.ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
				//var result = System.Threading.Tasks.Parallel.ForEach(roomGroups, parallelOpt, roomGroup =>
				//{
				//	if (hubContext != null)
				//		hubContext.Clients.Group(roomGroup.Group.Name).markInactive(roomGroup.Users).Wait();
				//});

				//foreach (var roomGroup in roomGroups)
				//{
				//	hubContext.Clients.Group(roomGroup.Group.Name).markInactive(roomGroup.Users).Wait();
				//}

				#endregion commented
			}
			return false;
		}

		internal static void DoMigrations(string conStrName)
		{
			var conString = ConfigurationManager.ConnectionStrings[conStrName];

			if (String.IsNullOrEmpty(conString.ProviderName) ||
				!conString.ProviderName.Equals(SqlClient, StringComparison.OrdinalIgnoreCase))
				return;
			// Only run migrations for SQL server (Sql ce not supported as yet)
			var settings = new WebSignalR.DataAccess.Migrations.Configuration();
			var migrator = new DbMigrator(settings);
			migrator.Update();
		}

		internal static void InitMapperMaps()
		{
			Mapper.CreateMap<User, UserDto>()
				.ForMember(dest => dest.Privileges, opt => opt.MapFrom(src => src.UserPrivileges))
				.ForMember(dest => dest.IsAdmin, opt => opt.MapFrom(src => src.IsAdmin))
				.ForSourceMember(src => src.ConnectedSessions, opt => opt.Ignore())
				.ForSourceMember(src => src.UserVotes, opt => opt.Ignore())
				.ForSourceMember(src => src.Password, opt => opt.Ignore());

			Mapper.CreateMap<UserDto, User>()
				.ForMember(dest => dest.UserPrivileges, opt => opt.MapFrom(src => src.Privileges))
				.ForMember(dest => dest.IsAdmin, opt => opt.MapFrom(src => src.IsAdmin))
				.ForMember(src => src.ConnectedSessions, opt => opt.Ignore())
				.ForMember(src => src.UserVotes, opt => opt.Ignore())
				.ForMember(src => src.Password, opt => opt.Ignore());

			Mapper.CreateMap<UserVote, UserVoteDto>()
				.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
				.ForMember(dest => dest.VoteItemId, opt => opt.MapFrom(src => src.VoteItem.Id))
				.ForMember(dest => dest.Mark, opt => opt.MapFrom(src => src.Mark))
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));


			Mapper.CreateMap<VoteItem, VoteItemDto>()
				.ForMember(d => d.Opened, opt => opt.MapFrom(s => s.Opened))
				.ForMember(dest => dest.HostRoomId, opt => opt.MapFrom(src => src.HostRoom.Id))
				.ForMember(dest => dest.VotedUsers, opt => opt.MapFrom(src => src.VotedUsers.Select(x => Mapper.Map<UserVoteDto>(x))))
				.IgnoreAllNonExisting();

			Mapper.CreateMap<VoteItemDto, VoteItem>()
				.ForMember(dest => dest.VotedUsers, opt => opt.MapFrom(src => src.VotedUsers.Select(x =>
					new UserVote() { Id = x.Id, Mark = x.Mark, UserId = x.UserId, VoteId = x.VoteItemId }).ToList()))
				.ForSourceMember(src => src.HostRoomId, opt => opt.Ignore())
				.IgnoreAllNonExisting();

			Mapper.CreateMap<Room, RoomDto>()
				.ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
				.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
				.ForMember(dest => dest.ItemsToVote, opt => opt.MapFrom(src => src.ItemsToVote))
				.ForMember(dest => dest.ConnectedUsers, opt => opt.MapFrom(src => src.ConnectedUsers));

			Mapper.CreateMap<RoomDto, Room>()
				.ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
				.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
				.ForMember(dest => dest.ItemsToVote, opt => opt.MapFrom(src => src.ItemsToVote))
				.ForMember(dest => dest.ConnectedUsers, opt => opt.MapFrom(src => src.ConnectedUsers));

			Mapper.CreateMap<Privileges, PrivilegeDto>().IgnoreAllNonExisting();
			Mapper.CreateMap<PrivilegeDto, Privileges>().IgnoreAllNonExisting();

			try
			{
				Mapper.AssertConfigurationIsValid();
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
			}
		}
	}
}