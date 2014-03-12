using System.Web.Mvc;
using WebSignalR.Common.Interfaces;
using Ninject;
using WebSignalR.Common.Entities;
using System.Linq;

namespace WebSignalR.Controllers
{
	public class RoomActivityController : Controller
	{
		public ActionResult Index(string roomName)
		{
			if (roomName != null)
			{
				IUnityOfWork unity = Infrastructure.BootStrapper.Kernel.Get<IUnityOfWork>();
				using (unity)
				{
					Room room = unity.GetRepository<Room>().Get(x => x.Name == roomName).FirstOrDefault();
					if (room != null)
					{
						ViewData["RoomId"] = room.Id;
						ViewData["RoomName"] = room.Name;
					}
				}
			}

			ViewData["User"] = User as Infrastructure.CustomPrincipal;
			ViewData["UserId"] = (User as Infrastructure.CustomPrincipal).UserId;
			ViewData["IsAdmin"] = (User as Infrastructure.CustomPrincipal).IsInRole("Admin");

			return View();
		}
	}
}
