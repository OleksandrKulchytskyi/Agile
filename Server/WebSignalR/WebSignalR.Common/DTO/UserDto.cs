using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.DTO
{
	public class UserDto : DtoBase
	{
		public UserDto()
		{
			Privileges = new List<PrivilegeDto>();
		}

		public string Name { get; set; }
		public bool IsAdmin { get; set; }

		public List<PrivilegeDto> Privileges { get; set; }
	}
}