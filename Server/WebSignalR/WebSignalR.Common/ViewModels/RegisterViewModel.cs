using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.ViewModels
{
	public class RegisterViewModel
	{
		[Required]
		[Display(Name = "User name")]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Retry Password")]
		public string RetryPassword { get; set; }
	}
}