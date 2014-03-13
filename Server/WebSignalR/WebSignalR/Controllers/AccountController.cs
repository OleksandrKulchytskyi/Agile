﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebSignalR.Common.Extension;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.ViewModels;
using WebSignalR.Infrastructure;
using Ninject;
using WebSignalR.Common.Entities;

namespace WebSignalR.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		[AllowAnonymous]
		[HttpPost]
		public JsonResult JsonLogin(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				System.Net.CookieContainer cookieContainer = new System.Net.CookieContainer();
				using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
				using (HttpClient client = new HttpClient(handler))
				{
					client.BaseAddress = GetBaseUrl();
					client.DefaultRequestHeaders.Add("Authorization", "Basic " + (model.Username + ":" + model.Password.toBase64Utf8()).toBase64Utf8());
					client.DefaultRequestHeaders.Add("Persistant", model.RememberMe ? "1" : "0");
					var task = client.GetAsync("handlers/loginhandler.ashx");
					try
					{
						task.Wait();
					}
					catch (AggregateException ex) { Global.Logger.Error(ex); }
					if (task.Result.StatusCode == System.Net.HttpStatusCode.OK)
					{
						if (cookieContainer != null)
						{
							var collection = cookieContainer.GetCookies(GetBaseUrl());
							foreach (System.Net.Cookie cookie in collection)
							{
								HttpCookie c = new HttpCookie(cookie.Name, cookie.Value);
								Response.Cookies.Add(c);
							}
						}
						return Json(new { success = true, redirect = returnUrl });
					}
					else
					{
						Session.Clear();
						HttpCookie c = Request.Cookies[Infrastructure.Constants.FormsAuthKey];
						if (c != null)
						{
							c = new HttpCookie(Infrastructure.Constants.FormsAuthKey);
							c.Expires = DateTime.Now.AddDays(-1);
							c.Value = string.Empty;
							Response.Cookies.Add(c);
						}
						ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
					}
				}
			}
			// If we got this far, something failed
			return Json(new { errors = GetErrorsFromModelState() });
		}

		private void SetPrincipal(Infrastructure.CustomPrincipal principal)
		{
			if (ControllerContext.HttpContext != null)
				ControllerContext.HttpContext.User = principal;
			System.Threading.Thread.CurrentPrincipal = principal;
		}

		private Uri GetBaseUrl()
		{
			return new Uri(Request.Url, Url.Content("~"));
		}

		private string GetDomain()
		{
			string domain1 = Request.Url.GetLeftPart(UriPartial.Authority);
			string domain2 = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? string.Empty : ":" + Request.Url.Port);
			return domain2;
		}

		private IEnumerable<string> GetErrorsFromModelState()
		{
			return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			System.Net.CookieContainer cookieContainer = new System.Net.CookieContainer();
			using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
			using (HttpClient client = new HttpClient(handler) { })
			{
				client.BaseAddress = GetBaseUrl();
				CustomIdentity identity = (User.Identity as CustomIdentity);
				if (identity != null)
				{
					string strEncryptedTicket = FormsAuthentication.Encrypt(identity.Ticket);
					string domain = Request.Url.Host;
					System.Net.Cookie cookie = new System.Net.Cookie(Infrastructure.Constants.FormsAuthKey, strEncryptedTicket, "/", domain);
					cookieContainer.Add(cookie);
				}
				var task = client.GetAsync("handlers/LogoutHandler.ashx");
				try
				{
					task.Wait();
				}
				catch (AggregateException ex) { Global.Logger.Error(ex); }
				if (task.Result.StatusCode == System.Net.HttpStatusCode.OK)
				{
					//return Json(new { success = true, redirect = returnUrl });
				}
				else
					ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
			}

			FormsAuthentication.SignOut();
			return RedirectToAction("Index", "Home");
		}


		public ActionResult Manage()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult JsonRegister(RegisterViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				// Attempt to register the user
				try
				{
					IUnityOfWork unity = Infrastructure.BootStrapper.Kernel.Get<IUnityOfWork>();
					using (unity)
					{
						IRepository<User> userRepo = unity.GetRepository<User>();
						if (userRepo.Get(x => x.Name == model.UserName).FirstOrDefault() != null)
							throw new InvalidOperationException("User with such name already exists.");
						if (model.Password != model.ConfirmPassword)
							throw new InvalidOperationException("Password must be the same.");

						User usr = new User();
						usr.Name = model.UserName;
						usr.Password = model.Password.toBase64Utf8();
						usr.UserPrivileges.Add(unity.GetRepository<Privileges>().Get(x => x.Name == "User").FirstOrDefault());
						userRepo.Add(usr);
						unity.Commit();
					}
					return Json(new { success = true, redirect = returnUrl });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			// If we got this far, something failed
			return Json(new { errors = GetErrorsFromModelState() });
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult JsonChangePassword(ChangePasswordViewModel model, string returnUrl)
		{
			if (ModelState.IsValid && User.Identity.IsAuthenticated)
			{
				try
				{
					IUnityOfWork unity = Infrastructure.BootStrapper.Kernel.Get<IUnityOfWork>();
					using (unity)
					{
						IRepository<User> userRepo = unity.GetRepository<User>();
						User usr;
						if ((usr = userRepo.Get(x => x.Name == User.Identity.Name).FirstOrDefault()) == null)
							throw new InvalidOperationException("User with such name is exists.");
						if (model.Password != model.ConfirmPassword)
							throw new InvalidOperationException("Password must be the same.");

						usr.Password = model.Password.toBase64Utf8();
						userRepo.Update(usr);
						unity.Commit();
					}
					return Json(new { success = true, redirect = returnUrl });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			// If we got this far, something failed
			return Json(new { errors = GetErrorsFromModelState() });
		}
	}
}