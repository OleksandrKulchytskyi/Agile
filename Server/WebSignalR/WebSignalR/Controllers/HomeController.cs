using System.Web.Mvc;

namespace WebSignalR.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}
	}
}