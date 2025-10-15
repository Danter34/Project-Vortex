using Microsoft.AspNetCore.Mvc;

namespace Vortex.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
