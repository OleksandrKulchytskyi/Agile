using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebSignalR.Common.DTO;
using WebSignalR.Common.Entities;
using WebSignalR.Common.Extension;
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
			Ensure.Argument.NotNull(unity, "unity");
			_unity = unity;
		}

		[HttpGet]
		public async Task<IEnumerable<PrivilegeDto>> GetPrivileges()
		{
			IEnumerable<PrivilegeDto> Dtos = Enumerable.Empty<PrivilegeDto>();
			try
			{
				Dtos = await
				TaskHelper.FromMethod<IEnumerable<PrivilegeDto>>(() =>
				{
					IReadOnlyRepository<Privileges> repo = _unity.GetRepository<Privileges>();
					IEnumerable<PrivilegeDto> privileges = repo.GetAll().Select(p => AutoMapper.Mapper.Map<PrivilegeDto>(p));
					return privileges;
				});
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) });
			}

			return Dtos;
		}

		[HttpPut]
		[ActionName("AppendUserRole")]
		public async Task<HttpResponseMessage> AppendUserRole(int userId, int privilegeId)
		{
			try
			{
				return await TaskHelper.FromMethod<HttpResponseMessage>(() =>
				{
					Privileges privilege;
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

					return Request.CreateResponse(HttpStatusCode.OK, AutoMapper.Mapper.Map<PrivilegeDto>(privilege));
				});
			}
			catch (Exception ex)
			{
				Global.Logger.Error(ex);
				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}