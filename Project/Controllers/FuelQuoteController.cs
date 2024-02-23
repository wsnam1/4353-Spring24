using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models;


namespace Project.Controllers
{
	public class FuelQuoteController : Controller
	{

		private readonly ApplicationDbContext _db;

		public FuelQuoteController(ApplicationDbContext db)
		{
			_db = db;
		}
		// GET: /FuelQuote
		public IActionResult Index()
		{
			return View();
		}
		// GET: /FuelQuote/History
		public IActionResult History()
		{
			List<FuelHistory> objFuelHistoryList = _db.FuelHistories.ToList();
			
			return View(objFuelHistoryList);
		}

		[HttpPost]

		public IActionResult Index(FuelHistory obj)
		{

			_db.FuelHistories.Add(obj);
			_db.SaveChanges();
			return RedirectToAction("Index", "Home");
		}
	}
}