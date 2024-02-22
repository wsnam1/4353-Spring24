using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Models;


namespace Project.Controllers
{
	public class FuelQuoteController : Controller
	{
		// GET: /FuelQuote
		public IActionResult Index()
		{
			return View();
		}
		// GET: /FuelQuote/History
		public IActionResult History()
		{
			return View();
		}
	}
}