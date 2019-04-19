using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coreapi.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private readonly IDataService _dataService;

		public ValuesController(IDataService ds)
		{
			_dataService = ds;
		}

		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			string user = string.IsNullOrEmpty(this.User.Identity.Name) ?
					"Anonymous" : this.User.Identity.Name;
			return new string[] { "value1", "value2", $"user: { user }" };
		}

		// GET api/values/5
		[HttpGet("{*path}")]
		public ActionResult<ReturnBox> Get(string path)
		{
			if (!_dataService.Connected)
				_dataService.Connect("192.168.99.100", 22, "san", @"C:\Users\san\Documents\WebApi\WebApi\id_rsa-webapi");

			var rb = _dataService.RunRemote($"ls -l /{path}");

			return rb;
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
			// For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
			// For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
			// For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see https://go.microsoft.com/fwlink/?LinkID=717803
		}
	}
}
