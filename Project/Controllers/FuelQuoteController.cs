using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Humanizer;
using System.Text.Json;

namespace Project.Controllers
{
	[Authorize]
    [ProfileCompletionFilter]
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
		public IActionResult GetQuote()
		{
            // Get the Id of the current logged-in user
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            // Use a ViewBag the delivery address coming the from the user's profile to display it in the view 
            if (userProfile != null)
			{
                ViewBag.DeliveryAddress = userProfile.Address1;
            }
            else
            {
                // Temporary else statement
                ViewBag.DeliveryAddress = "Null";
            }
			
			return View();
		}

        // POST Method
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetQuote([Bind("GallonsRequested,DeliveryAddress,DeliveryDate")] FuelHistory fuelQuote)
        {
            // Get the Id of the current logged-in user
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            // Since the UserId has a Key attribute and is not passed from the view, we can assign it programmatically and remove the model state error
            if (userId != null)
            {
                fuelQuote.UserId = userId;
                // This make the ModelState valid
                ModelState.Remove("UserId");
            }

            // Check if the model state is valid.
            if (ModelState.IsValid)
	        {
                // Calculate price
                Pricing pricing = new Pricing();
                bool inState = userProfile != null && userProfile.State == "TX";
                bool hasHistory = _context.FuelHistories.Any(f => f.UserId == userId);
                fuelQuote.SuggestedPrice = pricing.CalculatePrice(fuelQuote.GallonsRequested, inState, hasHistory);
                fuelQuote.TotalAmountDue = fuelQuote.SuggestedPrice * fuelQuote.GallonsRequested;

                // Store JSON string in TempData
                TempData["FuelQuoteJson"] = JsonSerializer.Serialize(fuelQuote);

                // If the model state is valid and the data is saved successfully, redirect the user to the Index action of the Home controller.
                return RedirectToAction("SubmitQuote", "FuelQuote");
            
            }

            // If saving data to db was unsuccessful we have to return the delivery address again to the view
            if (userProfile != null)
            {
                ViewBag.DeliveryAddress = userProfile.Address1;
            }
            else
            {
                // Temporary else statement
                ViewBag.DeliveryAddress = "Null";
            }

            // If the model state is not valid (e.g., required fields are missing or validation rules are not met), return the current view with the fuelQuote model. This allows the form to be re-displayed with the entered values and any validation messages.
            return View(fuelQuote);

        }

        [HttpGet]
        public IActionResult SubmitQuote()
        {
            if (TempData["FuelQuoteJson"] != null)
            {
                var userId = _userManager.GetUserId(User);

                // Retrieve JSON string from TempData
                var fuelQuoteJson = TempData["FuelQuoteJson"] as string;

                // Deserialize JSON string back to FuelHistory object
                var fuelQuote = JsonSerializer.Deserialize<FuelHistory>(fuelQuoteJson);

                // If saving data to db was unsuccessful we have to return the delivery address again to the view
                var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

                if (userProfile != null)
                {
                    ViewBag.DeliveryAddress = userProfile.Address1;
                }
                else
                {
                    // Temporary else statement
                    ViewBag.DeliveryAddress = "Null";
                }

                return View(fuelQuote);

            }
            return RedirectToAction("Index", "FuelQuote");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuote([Bind("GallonsRequested,DeliveryAddress,DeliveryDate,SuggestedPrice,TotalAmountDue")] FuelHistory fuelQuote)
        {
            // Get the Id of the current logged-in user
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            // Since the UserId has a Key attribute and is not passed from the view, we can assign it programmatically and remove the model state error
            if (userId != null)
            {
                fuelQuote.UserId = userId;
                // This make the ModelState valid
                ModelState.Remove("UserId");
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
            if (userProfile != null)
            {
                ViewBag.DeliveryAddress = userProfile.Address1;
            }
            else
            {
                // Temporary else statement
                ViewBag.DeliveryAddress = "Null";
            }

            // If the model state is not valid (e.g., required fields are missing or validation rules are not met), return the current view with the fuelQuote model. This allows the form to be re-displayed with the entered values and any validation messages.
            return View(fuelQuote);

        }

        // GET: /FuelQuote/History
        public IActionResult History()
        {
            // Step 1: Get the id of the user. We have some code above for this.
			var userId = _userManager.GetUserId(User);

            // Step 2: Get a list of all the fuel quote histories from the FuelHistories table in the db that match the user id
            // You can use the .Where() and .ToListAsync() methods for this

			var fuelQuotes = _context.FuelHistories.AsNoTracking().Where(f => f.UserId == userId).ToList();

            // Step 3: Pass the list of histories to the view using the return statement

            return View(fuelQuotes);
		}
        
        
        
	}
}