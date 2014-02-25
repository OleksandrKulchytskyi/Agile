using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.DTO
{
	public class PrivilegeDto : DtoBase
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
}