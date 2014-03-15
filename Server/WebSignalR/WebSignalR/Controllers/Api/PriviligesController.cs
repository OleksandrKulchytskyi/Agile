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
			if (User.Identity.IsAuthenticated)
			{
			}
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

		protected override void Dispose(bool disposing)
		{
			if (_unity != null)
				_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}