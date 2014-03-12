using System.Web.Optimization;

namespace WebSignalR
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/Site.css",
				"~/Content/Agile.css",
				"~/Content/bootstrap.css"));

			bundles.Add(new StyleBundle("~/Content/toastr").Include("~/Content/toastr.css"));

			bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
						"~/Content/themes/base/jquery.ui.core.css",
						"~/Content/themes/base/jquery.ui.resizable.css",
						"~/Content/themes/base/jquery.ui.selectable.css",
						"~/Content/themes/base/jquery.ui.accordion.css",
						"~/Content/themes/base/jquery.ui.autocomplete.css",
						"~/Content/themes/base/jquery.ui.button.css",
						"~/Content/themes/base/jquery.ui.dialog.css",
						"~/Content/themes/base/jquery.ui.slider.css",
						"~/Content/themes/base/jquery.ui.tabs.css",
						"~/Content/themes/base/jquery.ui.datepicker.css",
						"~/Content/themes/base/jquery.ui.progressbar.css",
						"~/Content/themes/base/jquery.ui.theme.css"));


			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
						"~/Scripts/jquery-ui-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.unobtrusive*",
						"~/Scripts/jquery.validate*"));

			bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
						"~/Scripts/knockout-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/ajaxlogin").Include(
				"~/Scripts/app/ajaxlogin.js"));

			bundles.Add(new ScriptBundle("~/bundles/toastr").Include(
				"~/Scripts/toastr.js"));

			bundles.Add(new ScriptBundle("~/bundles/notifyService").Include(
				"~/Scripts/services/notifyService.js"));

			bundles.Add(new ScriptBundle("~/bundles/agile").Include(
				"~/Scripts/app/common.bindings.js",
				"~/Scripts/app/agile.datacontext.js",
				"~/Scripts/app/agile.model.js",
				"~/Scripts/app/agile.viewmodel.js"));

			bundles.Add(new ScriptBundle("~/bundles/agileManage").Include(
				"~/Scripts/app/common.bindings.js",
				"~/Scripts/app/agile.datacontext.js",
				"~/Scripts/app/agile.model.js",
				"~/Scripts/app/account.viewmodel.js"));

			bundles.Add(new ScriptBundle("~/bundles/agileRoomActivity").Include(
				"~/Scripts/app/common.bindings.js",
				"~/Scripts/app/agile.datacontext.js",
				"~/Scripts/app/room.model.js",
				"~/Scripts/app/agile.roomactivityVM.js"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/ko-signalr").Include(
				"~/Scripts/jquery.signalR-{version}.js",
				"~/Scripts/knockout-{version}.js"));
		}
	}
}