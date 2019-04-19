using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coreapi.Data;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace coreapi.Controllers
{
	[Authorize]
	[Route("[controller]")]
	[ApiController]
	public class LinuxController : ControllerBase
	{
		private readonly IDataService _dataService;        

        public LinuxController(IDataService ds)
		{
			_dataService = ds;            
		}

		// GET linux/path
		[HttpGet("{*path}")]
		public ActionResult<ReturnBox> Index(string path)
		{
            UserModel user = _dataService.GetUser(User.Identity.Name);
            if(!user.IsSubscribedLinux)
            {
                return new ReturnBox {
                    Error = $"{user.Username} is not subscribed to this service.\n"
                };
            }
            var rb = _dataService.RunRemote(user.Ssh, $"ls -l /{path}");
            return rb;            
		}

		
	}
}
