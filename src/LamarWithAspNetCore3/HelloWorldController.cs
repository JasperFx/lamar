using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace LamarWithAspNetCore3
{
	[ApiController]
	[Route("[controller]")]
	public class MyController : ControllerBase
	{
		[HttpGet("helloworld")]
		public string GetHelloWorld()
		{
			return "Hello World!";
		}
	}
}
