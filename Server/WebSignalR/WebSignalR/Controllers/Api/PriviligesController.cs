using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Interfaces;
using WebSignalR.Infrastructure.Authorization;

namespace WebSignalR.Controllers
{
	[WebApiAuth(Roles = "Admin")]
	public class PrivilegesController : BaseController
	{
		private readonly IUnityOfWork _unity;

		public PrivilegesController(IUnityOfWork unity)
		{
			_unity = unity;
		}

		[HttpGet]
		public IEnumerable<PrivilegeDto> GetPrivileges()
		{
			try
			{
				IReadOnlyRepository<Privileges> repo = _unity.GetRepository<Privileges>();
				IEnumerable<PrivilegeDto> privileges = repo.GetAll().Select(p => AutoMapper.Mapper.Map<PrivilegeDto>(p));
				return privileges;
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}
		}

		[HttpPut]
		[ActionName("AppendUserRole")]
		public HttpResponseMessage AppendUserRole(int userId, int privilegeId)
		{
			Privileges privilege;
			try
			{
				IReadOnlyRepository<Privileges> privRepo = _unity.GetRepository<Privileges>();
				IRepository<User> userRepo = _unity.GetRepository<User>();
				User user = userRepo.Get(x => x.Id == userId).FirstOrDefault();
				privilege = privRepo.Get(x => x.Id == privilegeId).FirstOrDefault();
				if (user != null && privilege != null)
				{
					user.UserPrivileges.Add(privilege);
					userRepo.Update(user);
					_unity.Commit();
				}
				else
					Request.CreateErrorResponse(HttpStatusCode.NotFound, "Entities were not found.");

			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
			}
			return Request.CreateResponse(HttpStatusCode.OK, AutoMapper.Mapper.Map<PrivilegeDto>(privilege));
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}