using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
