using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace Project.Controllers
{
	[Authorize]
	public class FuelQuoteController : Controller
	{

		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public FuelQuoteController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		// GET: /FuelQuote
		public IActionResult Index()
		{
			var userId = _userManager.GetUserId(User);
			var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

			if (userProfile == null)
			{
				return View("Error");
			}
			
			var viewModel = new FuelQuoteViewModel
			{
				UserProfile = userProfile,
				FuelHistory = new FuelHistory()

			};
			return View(viewModel);
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