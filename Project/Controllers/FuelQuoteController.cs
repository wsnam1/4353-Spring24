using Microsoft.AspNetCore.Mvc;
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