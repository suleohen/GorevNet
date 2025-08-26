using Microsoft.AspNetCore.Mvc;

namespace GorevNet.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
