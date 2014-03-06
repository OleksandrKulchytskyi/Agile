﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.ViewModels;

namespace WebSignalR.Controllers
{
	[Authorize]
	public class UserController : BaseController
	{
		private IUnityOfWork _unity;
		
		public UserController(IUnityOfWork unity)
		{
			_unity = unity;
		}

		[AllowAnonymous]
		//[Authorize(Roles = "Admin")]
		public HttpResponseMessage RegisterUser(RegisterViewModel model)
		{
			IRepository<User> repo = _unity.GetRepository<User>();
			if (repo.Exist(x => x.Name == model.UserName))
				return new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("User with such name already exists.") };

			else if (model.Password != model.ConfirmPassword)
				return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Password must be the same.") };
			try
			{
				User usr = new User()
				{
					Name = model.UserName,
					Password = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.Password)),
					IsAdmin = false
				};
				usr.UserPrivileges.Add(_unity.GetRepository<Privileges>().Get(x => x.Name == "User").First());
				repo.Add(usr);
				_unity.Commit();
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.Created);
			msg.Headers.Add("Id", repo.Get(x => x.Name == model.UserName).First().Id.ToString());
			return msg;
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}