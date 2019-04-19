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
            if (!u.IsSubscribedLinux)
            {
                if (ModelState.IsValid)
                {
                    usermodel.Username = u.Username;
                    _dataService.SubscribeLinux(usermodel);
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