using GorevNet.Context;
using Microsoft.AspNetCore.Mvc;

namespace GorevNet.Controllers
{
    public class AdminController : Controller
    {
        private readonly TasksDBContext _context;

        public AdminController(TasksDBContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ActiveTasks()
        {
            var tasks = _context.UserTasks.ToList();
            return View(tasks);
        }

    }
}
