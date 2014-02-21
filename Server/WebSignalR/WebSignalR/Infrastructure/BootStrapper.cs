using Microsoft.AspNet.SignalR;
using Ninject;
using System;
using System.Configuration;
using System.Data.Entity.Migrations;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Infrastructure;
using WebSignalR.Common.Interfaces;
using WebSignalR.DataAccess.DB;
using WebSignalR.DataAccess.Repositories;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebSignalR.Infrastructure.BootStrapper), "PreAppStart")]
namespace WebSignalR.Infrastructure
{
	public static class BootStrapper
	{
		private static bool _sweeping;
		private static bool _broadcasting;

		private static readonly TimeSpan _sweepInterval = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan _broadcatsInterval = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["broadcastTime"]));
		private const string SqlClient = "System.Data.SqlClient";

		internal static IKernel Kernel = null;
		private static IDependencyResolver _resolver = null;

		public static void PreAppStart()
		{
			var kernel = new StandardKernel();

			IniDIMappings(kernel);

			var serializer = new Microsoft.AspNet.SignalR.Json.JsonNetSerializer(new Newtonsoft.Json.JsonSerializerSettings
			{
				DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat,
			});

			Kernel = kernel;
			_resolver = new DependencyResolvers.NinjectDependencyResolver(kernel);
			App_Start.SignalRConfig.Register(_resolver);
		}

		private static void IniDIMappings(StandardKernel kernel)
		{
			kernel.Bind<DatabaseContext>().To<DatabaseContext>();
			kernel.Bind<IContext>().To<DatabaseContext>();
			kernel.Bind<IRepository<Room>>().To<GenericRepository<Room>>();
			kernel.Bind<IRepository<User>>().To<GenericRepository<User>>();
			kernel.Bind<IRepository<UserSession>>().To<GenericRepository<UserSession>>();
			kernel.Bind<IRepository<Privileges>>().To<GenericRepository<Privileges>>();
			kernel.Bind<IRepository<VoteItem>>().To<GenericRepository<VoteItem>>();
			kernel.Bind<IRepository<UserVote>>().To<GenericRepository<UserVote>>();

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
			kernel.Bind<IVotesProvider>().ToConstant(new FileBasedVotesProvider());

			kernel.Bind<Hubs.TestHub>().ToMethod(context =>
			{
				// I'm doing this manually, since we want the repository instance to be shared between the messanger service and the messanger hub itself
				var unity = context.Kernel.Get<IUnityOfWork>();
				var crypto = context.Kernel.Get<ICryptoService>();

				//var service = new MessangerService(cache, crypto, repository);
				return new Hubs.TestHub(unity, crypto);
			});
		}


		public static void DoMigrations(string conStrName)
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
	}
}