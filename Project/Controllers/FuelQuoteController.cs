using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;


namespace Project.Controllers
{
	public class FuelQuoteController : Controller
	{

		private readonly ApplicationDbContext _context;

		public FuelQuoteController(ApplicationDbContext context)
		{
			_context = context;
		}
		// GET: /FuelQuote
		public IActionResult Index()
		{
			return View();
		}

        // POST Method
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("GallonsRequested,DeliveryDate")] FuelHistory fuelQuote)
        {
            // Check if the model state is valid.
            if (ModelState.IsValid)
            {
                // This line doesn't insert the fuelQuote into the database yet; it just tracks the object in the EF context for further operations.
                _context.Add(fuelQuote);

                // Asynchronously save all changes made in the context to the database. This call actually inserts the fuelQuote into the database.
                await _context.SaveChangesAsync();

                // If the model state is valid and the data is saved successfully, redirect the user to the Index action of the Home controller.
                return RedirectToAction("Index", "Home");
            }

            // If the model state is not valid (e.g., required fields are missing or validation rules are not met), return the current view with the fuelQuote model. This allows the form to be re-displayed with the entered values and any validation messages.
            return View(fuelQuote);


        }

        // GET: /FuelQuote/History
        public async Task<IActionResult> History()
		{
            // Asynchronously return all the rows in the FuelHistories table to the view.
            return View(await _context.FuelHistories.ToListAsync());
		}
	}
}