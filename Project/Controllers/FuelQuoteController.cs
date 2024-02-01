using Microsoft.AspNetCore.Mvc;
namespace Project.Controllers
{

    public class FuelQuoteController : Controller
    {
        
        private readonly ILogger<FuelQuoteController> _logger;

        public FuelQuoteController(ILogger<FuelQuoteController> logger)
        {
            // Used to log and debug info
            _logger = logger;
        }
        // GET: /FuelQuote
        public IActionResult Index()
        {
            return View();
        }


        
    }
}