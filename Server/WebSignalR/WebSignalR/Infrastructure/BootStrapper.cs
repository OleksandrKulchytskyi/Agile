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
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Services;
using WebSignalR.DataAccess.DB;
using WebSignalR.DataAccess.Repositories;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebSignalR.Infrastructure.BootStrapper), "PreAppStart")]

namespace WebSignalR.Infrastructure
{
	public static class BootStrapper
	{
		private static bool _sweeping;
		private static Timer _timerClaen;
		private static readonly TimeSpan _sweepInterval = TimeSpan.FromMinutes(15);
		private const string SqlClient = "System.Data.SqlClient";

		internal static IKernel Kernel = null;
		private static IDependencyResolver _signalrResolver = null;

		public static void PreAppStart()
		{
			var kernel = new StandardKernel();

			IniDIContainer(kernel);

			Kernel = kernel;
			_signalrResolver = new DependencyResolvers.NinjectSignalRDependencyResolver(kernel);
			App_Start.SignalRConfig.Register(_signalrResolver);

			System.Web.Mvc.DependencyResolver.SetResolver(new DependencyResolvers.NInjectMvcDependencyResolver(kernel));
			var unityFactory = new Func<IUnityOfWork>(() => kernel.Get<IUnityOfWork>());
			_timerClaen = new Timer(_ => Sweep(unityFactory, _signalrResolver), null, _sweepInterval, _sweepInterval);
		}

		private static void IniDIContainer(StandardKernel kernel)
		{
			kernel.Bind<System.Web.Mvc.IControllerActivator>().To<DependencyResolvers.NinjectMvcControllerActivator>();

			kernel.Bind<Microsoft.AspNet.SignalR.Hubs.IHubConnectionContext>().ToMethod(context =>
						_signalrResolver.Resolve<Microsoft.AspNet.SignalR.Infrastructure.IConnectionManager>().GetHubContext<Hubs.AgileHub>().Clients).
						Named("AgileHub");

			kernel.Bind<DatabaseContext>().To<DatabaseContext>();
			kernel.Bind<IContext>().To<DatabaseContext>();
			kernel.Bind<IRepository<Room>>().To<GenericRepository<Room>>();
			kernel.Bind<IRepository<User>>().To<GenericRepository<User>>();
			kernel.Bind<IRepository<UserSession>>().To<GenericRepository<UserSession>>();
			kernel.Bind<IRepository<Privileges>>().To<GenericRepository<Privileges>>();
			kernel.Bind<IRepository<VoteItem>>().To<GenericRepository<VoteItem>>();
			kernel.Bind<IRepository<UserVote>>().To<GenericRepository<UserVote>>();

			kernel.Bind<IEntityValidator>().To<UserCredentialsValidator>().Named("CredentialsValidator");
			kernel.Bind<IPrincipalProvider>().To<Infrastructure.Services.FormsPrincipalProvider>().InSingletonScope();

			kernel.Bind<IUnityOfWork>().ToMethod(context =>
			{
				var room = kernel.Get<IRepository<Room>>();
				var user = kernel.Get<IRepository<User>>();
				var session = kernel.Get<IRepository<UserSession>>();
				var privileges = kernel.Get<IRepository<Privileges>>();
				var vote = kernel.Get<IRepository<VoteItem>>();
				var uv = kernel.Get<IRepository<UserVote>>();
				IContext ctx = kernel.Get<IContext>();

				return new WebSignalR.DataAccess.Repositories.UnityOfWork(user, room, privileges, vote, session, uv, ctx);
			});

			kernel.Bind<ICryptoService>().To<CryptoService>().InSingletonScope();
			kernel.Bind<IKeyProvider>().ToConstant(new FileBasedKeyProvider());
			kernel.Bind<IVotesProvider>().ToConstant(new FileBasedVotesProvider()).Named("FileBased");
			kernel.Bind<IUserRoomService>().To<UserRoomService>();
			kernel.Bind<ISessionService>().To<Infrastructure.Services.SessionService>();

			kernel.Bind<Hubs.AgileHub>().ToMethod(context =>
			{
				IUnityOfWork unity = context.Kernel.Get<IUnityOfWork>();
				ICryptoService crypto = context.Kernel.Get<ICryptoService>();
				IUserRoomService userRoomSrv = context.Kernel.Get<IUserRoomService>();
				ISessionService sessions = context.Kernel.Get<ISessionService>();

				return new Hubs.AgileHub(unity, crypto, userRoomSrv, sessions);
			});

			var service = new Func<ISessionService>(() => kernel.Get<ISessionService>());
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
				if (elapsed.TotalMinutes > 60)
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


			Mapper.CreateMap<VoteItem, VoteItemDto>()//TODO: check here !! in JS model ve got only integers, needs to be redesigned
				.ForMember(d => d.Opened, opt => opt.MapFrom(s => s.Opened))
				.ForMember(dest => dest.VotedUsers, opt => opt.MapFrom(src => src.VotedUsers.Select(x => x.UserId).ToList()))
				.IgnoreAllNonExisting();

			Mapper.CreateMap<VoteItemDto, VoteItem>()
				.ForMember(dest => dest.VotedUsers, opt => opt.MapFrom(src => src.VotedUsers.Select(x => new UserVote() { Id = x }).ToList()))
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