using Microsoft.AspNetCore.Mvc.Rendering;

namespace GorevNet.Models.ViewModels
{
    public class EditTaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Comment { get; set; }
        public int AssignedUserId { get; set; }

        // Dropdown için çalışan listesi
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    }
}
