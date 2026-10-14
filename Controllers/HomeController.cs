using Microsoft.AspNetCore.Mvc;

namespace PhissingScaming.Controllers
{
    // Home controller: Index (Dashboard), Modules (Awareness), Resources
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Modules
        public IActionResult Modules()
        {
            return View();
        }

        // GET: /Home/Resources
        public IActionResult Resources()
        {
            return View();
        }
    }
}
