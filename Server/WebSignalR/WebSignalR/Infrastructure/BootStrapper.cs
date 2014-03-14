using AutoMapper;
using Microsoft.AspNet.SignalR;
using Ninject;
using System;
using System.Linq;
using System.Configuration;
using System.Data.Entity.Migrations;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;
using WebSignalR.DataAccess.DB;
using WebSignalR.DataAccess.Repositories;
using WebSignalR.Common.Services;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebSignalR.Infrastructure.BootStrapper), "PreAppStart")]
namespace WebSignalR.Infrastructure
{
	public static class BootStrapper
	{
		//private static bool _sweeping;
		//private static bool _broadcasting;

		private static readonly TimeSpan _sweepInterval = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan _broadcatsInterval = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["broadcastTime"]));
		private const string SqlClient = "System.Data.SqlClient";

		internal static IKernel Kernel = null;
		private static IDependencyResolver _resolver = null;

		public static void PreAppStart()
		{
			var kernel = new StandardKernel();

			IniDIContainer(kernel);

			Kernel = kernel;
			_resolver = new DependencyResolvers.NinjectSignalRDependencyResolver(kernel);
			App_Start.SignalRConfig.Register(_resolver);

			System.Web.Mvc.DependencyResolver.SetResolver(new DependencyResolvers.NInjectMvcDependencyResolver(kernel));
		}

		private static void IniDIContainer(StandardKernel kernel)
		{
			kernel.Bind<System.Web.Mvc.IControllerActivator>().To<DependencyResolvers.NinjectMvcControllerActivator>();

			kernel.Bind<DatabaseContext>().To<DatabaseContext>();
			kernel.Bind<IContext>().To<DatabaseContext>();
			kernel.Bind<IRepository<Room>>().To<GenericRepository<Room>>();
			kernel.Bind<IRepository<User>>().To<GenericRepository<User>>();
			kernel.Bind<IRepository<UserSession>>().To<GenericRepository<UserSession>>();
			kernel.Bind<IRepository<Privileges>>().To<GenericRepository<Privileges>>();
			kernel.Bind<IRepository<VoteItem>>().To<GenericRepository<VoteItem>>();
			kernel.Bind<IRepository<UserVote>>().To<GenericRepository<UserVote>>();
			kernel.Bind<IEntityValidator>().To<UserCredentialsValidator>().Named("CredentialsValidator");

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
			kernel.Bind<IPrincipalProvider>().To<Infrastructure.Services.FormsPrincipalProvider>().InSingletonScope();

			kernel.Bind<Hubs.AgileHub>().ToMethod(context =>
			{
				// I'm doing this manually, since we want the repository instance to be shared between the messanger service and the messanger hub itself
				IUnityOfWork unity = context.Kernel.Get<IUnityOfWork>();
				ICryptoService crypto = context.Kernel.Get<ICryptoService>();
				IUserRoomService userRoomSrv = context.Kernel.Get<IUserRoomService>();
				//var service = new MessangerService(cache, crypto, repository);
				return new Hubs.AgileHub(unity, crypto, userRoomSrv);
			});
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
				.ForMember(dest => dest.VoteItemId, opt => opt.MapFrom(src => src.VoteItem.Id));

			Mapper.CreateMap<VoteItem, VoteItemDto>()
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