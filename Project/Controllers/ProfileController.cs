using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;

namespace Project.Controllers
{
    // This will prompt the user to login first before accessing the Profile pages
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            if (userProfile != null)
            {
                return RedirectToAction("Edit", "Profile");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Address1,Address2,State,City,Zipcode")] UserProfile userProfile)
        {
            var userId = _userManager.GetUserId(User);

            if (userId != null)
            {
                userProfile.UserId = userId;
                ModelState.Remove("UserId");
            }

            if (ModelState.IsValid)
            {
                _context.Add(userProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(userProfile);
        }

        public IActionResult Edit()
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            if (userProfile == null)
            {
                return RedirectToAction("Create", "Profile");
            }

            return View(userProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("FullName,Address1,Address2,State,City,Zipcode")] UserProfile editedUserProfile)
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = _context.UserProfiles.SingleOrDefault(p => p.UserId == userId);

            if (userId != null &&  userProfile != null)
            {
                userProfile.FullName = editedUserProfile.FullName;
                userProfile.Address1 = editedUserProfile.Address1;
                userProfile.Address2 = editedUserProfile.Address2;
                userProfile.State = editedUserProfile.State;
                userProfile.City = editedUserProfile.City;
                userProfile.Zipcode = editedUserProfile.Zipcode;
                ModelState.Remove("UserId");

            }

            if (ModelState.IsValid)
            {
                
                _context.Update(userProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(userProfile);
        }



        // POST Edit
    }
}
