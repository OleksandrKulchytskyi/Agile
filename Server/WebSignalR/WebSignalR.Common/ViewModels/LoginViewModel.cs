﻿using System.ComponentModel.DataAnnotations;

namespace WebSignalR.Common.ViewModels
{
	public class LoginViewModel
	{
		[Required]
		[Display(Name = "User name")]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	public class CustomPrincipalSerializeModel
	{
		public int UserId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string[] roles { get; set; }
	}
}