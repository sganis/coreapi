using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;

namespace coreapi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDataService _dataService;

        public HomeController(IDataService ds)
        {
            _dataService = ds;

        }
        // GET: Subscribe
        public ActionResult Index()
        {

            HomeModel home = new HomeModel();
            home.Username = User.Identity.Name;
            if (string.IsNullOrEmpty(home.Username))
            {                
                home.Username = "Anonymous user";                
            }
            return View(home);
            //return RedirectToAction("Home");
        }

        [HttpPost]
        public IActionResult Index(HomeModel login)
        {
            if (ModelState.IsValid)
            {
                login.Username = "user";
                var ok = _dataService.Connect("192.168.99.100", 
                    22, 
                    "san", 
                    @"C:\Users\san\Documents\WebApi\WebApi\id_rsa-webapi");
                if(ok)
                {
                    
                }

                //return View(login);
                //_feedbackRepository.AddFeedback(feedback);
                //return RedirectToAction("Home", new { @login=login });
                //return RedirectToAction("Home");
            }

            return View(login);
        }

       
    }
}