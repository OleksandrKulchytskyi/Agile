using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebSignalR.Common.ViewModels;
using WebSignalR.Common.Extension;

namespace WebSignalR.Controllers
{
	[Authorize]
	//[InitializeSimpleMembership]
	public class AccountController : Controller
	{
		[AllowAnonymous]
		[HttpPost]
		public JsonResult JsonLogin(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				using (HttpClient client = new HttpClient()) 
				{
					client.BaseAddress = GetBaseUrl();
					client.DefaultRequestHeaders.Add("Authorization", "Basic " + (model.Username + ":" + model.Password.toBase64Utf8()).toBase64Utf8());
					var task = client.GetAsync("/handlers/loginhandler.ashx");
					try
					{
						task.Wait();
					}
					catch (AggregateException ex) { }
					if (task.Result.StatusCode == System.Net.HttpStatusCode.OK)
					{
						return Json(new { success = true, redirect = returnUrl });
					}
					else
						ModelState.AddModelError("", "The user name or password provided is incorrect.");
				}

				//if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
				//{
				//	FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
				//	return Json(new { success = true, redirect = returnUrl });
				//}
				//else
				//{
				//	ModelState.AddModelError("", "The user name or password provided is incorrect.");
				//}
			}

			// If we got this far, something failed
			return Json(new { errors = GetErrorsFromModelState() });
		}

		private Uri GetBaseUrl()
		{
			return new Uri(Request.Url, Url.Content("~"));
		}

		private IEnumerable<string> GetErrorsFromModelState()
		{
			return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			using (HttpClient client = new HttpClient())
			{
				//client.DefaultRequestHeaders.Add("Authorization", "Basic " + (model.Username + ":" + model.Password.toBase64Utf8()).toBase64Utf8());
				var task = client.GetAsync("/handlers/LogoutHandler.ashx");
				try
				{
					task.Wait();
				}
				catch (AggregateException ex) { }
				if (task.Result.StatusCode == System.Net.HttpStatusCode.OK)
				{
					//return Json(new { success = true, redirect = returnUrl });
				}
				else
					ModelState.AddModelError("", "The user name or password provided is incorrect.");
			}

			return RedirectToAction("Index", "Home");
		}
	}
}