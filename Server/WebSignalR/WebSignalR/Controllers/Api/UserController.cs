using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.ViewModels;

namespace WebSignalR.Controllers
{
	public class UserController : BaseController
	{
		private readonly IUnityOfWork _unity;

		public UserController(IUnityOfWork unity)
		{
			_unity = unity;
		}

		[ActionName("GetUserPrivileges")]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
		[HttpGet]
		public HttpResponseMessage GetUserPrivileges(int userId)
		{
			IReadOnlyRepository<User> userRepo = _unity.GetRepository<User>();
			var user = userRepo.Get(x => x.Id == userId).FirstOrDefault();
			if (user == null)
				return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User with such id cannot be found.");

			return Request.CreateResponse(HttpStatusCode.OK, user.UserPrivileges.Select(x => AutoMapper.Mapper.Map<PrivilegeDto>(x)).ToList());
		}


		[HttpPost]
		[ActionName("RegisterUser")]
		[Infrastructure.Filters.ValidateHttpAntiForgeryToken]
		[Infrastructure.Authorization.WebApiAuth(Roles = "Admin")]
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
					Password = model.Password.toBase64Utf8(),
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

			UserDto dto = AutoMapper.Mapper.Map<UserDto>(repo.Get(x => x.Name == model.UserName).FirstOrDefault());
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, dto);
			response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = dto.Id }));
			return response;
		}

		[ActionName("ChangePassword")]
		[HttpPut()]
		[Infrastructure.Filters.ValidateHttpAntiForgeryToken]
		[Infrastructure.Authorization.WebApiAuth(Roles = "User")]
		public HttpResponseMessage ChangePassword(ChangePasswordViewModel model)
		{
			IRepository<User> repo = _unity.GetRepository<User>();
			WebSignalR.Common.Entities.User usr;
			if ((usr = repo.Get(x => x.Name == User.Identity.Name).FirstOrDefault()) == null)
				return new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("User is not exists.") };

			else if (model.Password != model.ConfirmPassword)
				return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Password must be the same.") };
			try
			{
				usr.Password = model.Password.toBase64Utf8();
				repo.Update(usr);
				_unity.Commit();
			}
			catch (System.Exception ex)
			{
				Global.Logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			UserDto dto = AutoMapper.Mapper.Map<UserDto>(usr);
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, dto);
			response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = dto.Id }));
			return response;
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}