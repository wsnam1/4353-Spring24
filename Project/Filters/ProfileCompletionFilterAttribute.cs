using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Project.Data;

public class ProfileCompletionFilterAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Access UserManager and DbContext from the DI system
        var userManager = context.HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
        var dbContext = context.HttpContext.RequestServices.GetService<ApplicationDbContext>();

        // Extract the user from the HttpContext
        var user = await userManager.GetUserAsync(context.HttpContext.User);
        if (user != null)
        {
            // Check if the user profile is complete
            var userId = await userManager.GetUserIdAsync(user);
            var userProfileComplete = dbContext.UserProfiles.Any(up => up.UserId == userId);

            if (!userProfileComplete)
            {
                // If profile is not complete, redirect to profile form page
                context.Result = new RedirectToActionResult("Create", "Profile", null);
                return;
            }
        }

        // Proceed with the pipeline if profile is complete or user is not logged in
        await next();
    }
}