using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSignalR.Common.DTO;

namespace WebSignalR.ViewModels
{
	public class ChangeRolesViewModel
	{
		public List<UserVM> UserList
		{
			get;
			set;
		}

		public List<PrivilegeDto> AvaliableRoles
		{
			get;
			set;
		}
	}

	public class UserVM
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}