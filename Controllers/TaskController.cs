using Microsoft.AspNetCore.Mvc;

namespace GorevNet.Controllers
{
    public class TaskController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
