using GorevNet.Models;

namespace GorevNet.Models.ViewModels
{
    public class TaskDisplayViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Comment { get; set; }
        public int AssignedUserId { get; set; }
        public string AssignedUserName { get; set; } // Kullanıcının adı
        public string CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Status için Türkçe açıklama
        public string StatusDisplay => Status switch
        {
            TaskStatus.Beklemede => "Beklemede",
            TaskStatus.DevamEdiyor => "Devam Ediyor",
            TaskStatus.Tamamlandı => "Tamamlandı",
            _ => "Bilinmiyor"
        };

        // Priority için Türkçe açıklama
        public string PriorityDisplay => Priority switch
        {
            TaskPriority.Düşük => "Düşük",
            TaskPriority.Normal => "Normal",
            TaskPriority.Yüksek => "Yüksek",
            _ => "Normal"
        };
    }
}