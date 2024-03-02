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
            // Use a ViewBag the delivery address coming the from the user's profile to display it in the view 
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

			if (userProfile != null)
			{
                ViewBag.DeliveryAddress = userProfile.Address1;
            }
			
			return View();
		}

        // POST Method
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("GallonsRequested,DeliveryAddress,DeliveryDate,SuggestedPrice,TotalAmountDue")] FuelHistory fuelQuote)
        {
            // Get the Id of the current logged-in user
            var userId = _userManager.GetUserId(User);


            // Assign values programmatically

            if (userId != null)
			{
                fuelQuote.UserId = userId;
				ModelState.Remove("UserId"); // Remove ModelState errors for UserId
            }

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

            // If saving data to db was unsuccessful we have to return the delivery address again to the view
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            if (userProfile != null)
            {
                ViewBag.DeliveryAddress = userProfile.Address1;
            }

            // If the model state is not valid (e.g., required fields are missing or validation rules are not met), return the current view with the fuelQuote model. This allows the form to be re-displayed with the entered values and any validation messages.
            return View(fuelQuote);

        }

        // GET: /FuelQuote/History
        public async Task<IActionResult> History()
        {

            // Step 1: Get the id of the user

            // Step 2: Get a list of all the fuel quote histories from the FuelHistories table in the db that match the user id
            // You can use the .Where() and .ToListAsync() methods for this

            // Step 3: Pass the list of histories to the view using the return statement

            return View();
		}
	}
}