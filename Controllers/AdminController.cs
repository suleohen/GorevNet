using GorevNet.Context;
using GorevNet.Models;
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

        [HttpGet]
        public IActionResult CreateTask()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTask(UserTask model)
        {
            _context.UserTasks.Add(model);
            _context.SaveChanges();
            return RedirectToAction("ActiveTasks");
        }

        [HttpGet]
        public IActionResult EditTask(int id)
        {
            var task = _context.UserTasks.Where(x => x.Id == id).FirstOrDefault();
            return View(task);
        }

        [HttpPost]
        public IActionResult EditTask(UserTask model)
        { 
            var existingTask = _context.UserTasks.Where(x=>x.Id == model.Id).FirstOrDefault();
            
            existingTask.Title = model.Title;
            existingTask.Description = model.Description;
            existingTask.DueDate = model.DueDate;
            existingTask.Status = model.Status;
            existingTask.Priority = model.Priority;
            existingTask.Comment = model.Comment;
            existingTask.AssignedUserId = model.AssignedUserId;

            _context.SaveChanges();
            return RedirectToAction("ActiveTasks");

    }

        public IActionResult DeleteTask(int id)
        {
            var task= _context.UserTasks.Where(x => x.Id == id).FirstOrDefault();
            _context.UserTasks.Remove(task);
            _context.SaveChanges();
            return RedirectToAction("ActiveTasks");
        }


    }
  
}
