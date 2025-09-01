using Microsoft.AspNetCore.Mvc;

namespace GorevNet.Controllers
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
