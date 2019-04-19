using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using coreapi.Data;
using coreapi.Utilities;

namespace coreapi.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IDataService _dataService;

        public UserController(IDataService ds)
        {
            _dataService = ds;
        }

		// GET: index
        public IActionResult Index()
        {
            UserModel user = _dataService.GetUser(User.Identity.Name);
            return View(user);
        }

        [HttpPost]
        public ActionResult SubscribeLinux(UserModel usermodel)
        {
            UserModel u = _dataService.GetUser(User.Identity.Name);
            usermodel.Username = u.Username;
            if (!u.IsSubscribedLinux)
            {
                if (ModelState.IsValid)
                {
                    var rb = _dataService.SubscribeLinux(usermodel);
                    //TempData["error"] = rb.Error;
                    if (!string.IsNullOrEmpty(rb.Error))
                    {
                        ModelState.AddModelError("LinuxPassword", rb.Error);
                        return View("Index", usermodel);
                    }
                    return RedirectToAction("Index");
                }
            }
            return View("Index", usermodel);

        }
        [HttpPost]
        public ActionResult UnsubscribeLinux(UserModel usermodel)
        {
            usermodel.Username = User.Identity.Name;
            _dataService.UnsubscribeLinux(usermodel);
            return RedirectToAction("Index");            
        }
    }
}