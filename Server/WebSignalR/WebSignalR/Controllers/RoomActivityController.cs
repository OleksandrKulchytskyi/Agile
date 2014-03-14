using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Controllers
{
	[Authorize]
	public class RoomActivityController : MvcController
	{
		public RoomActivityController(IUnityOfWork unity)
		{
			base._unity = unity;
		}

		public ActionResult Index(string roomName)
		{
			if (roomName != null)
			{
				Room room = Unity.GetRepository<Room>().Get(x => x.Name == roomName).FirstOrDefault();
				if (room != null)
				{
					ViewData["RoomId"] = room.Id;
					ViewData["RoomName"] = room.Name;
					Session["RoomId"] = room.Id;
					Session["RoomName"] = room.Name;
				}
			}

			ViewData["User"] = User as Infrastructure.CustomPrincipal;
			ViewData["UserId"] = (User as Infrastructure.CustomPrincipal).UserId;
			ViewData["IsAdmin"] = (User as Infrastructure.CustomPrincipal).IsInRole("Admin");

			return View();
		}

		[Authorize]
		[HttpPost]
		public ActionResult Upload(FormCollection formCollection)
		{
			if (Request != null)
			{
				int rid = -1;
				if (Request.Headers.AllKeys.Contains("RoomId"))
				{
					rid = int.Parse(Request.Headers.Get("roomId"));
				}
				else if (Session["RoomId"] != null)
				{
					rid = (int)Session["RoomId"];
				}

				HttpPostedFileBase file = Request.Files["UploadedFile"];
				if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
				{
					if (rid != -1)
					{
						try
						{
							IVotesProvider provider = new Infrastructure.XmlVotesProvider();
							provider.Source = file.InputStream;
							ICollection<VoteItem> data = provider.GetVotes();
							Room room = Unity.GetRepository<Room>().Get(x => x.Id == rid).FirstOrDefault();
							foreach (var item in data)
							{
								item.HostRoom = room;
								room.ItemsToVote.Add(item);
							}
							Unity.Commit();

							RoomDto dto = AutoMapper.Mapper.Map<RoomDto>(room);
							GetHub<Hubs.AgileHub>().Clients.Group(dto.Name).onRoomStateChanged(dto);
						}
						catch (Exception ex)
						{
							return Json(new { Status = ex.Message });
						}
					}
					return Json(new { Status = "Ok" });
				}
				return Json(new { Status = "No file" });
			}
			else
				return Json(new { Status = "Failed" });
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
