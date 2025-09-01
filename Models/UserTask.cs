using System.ComponentModel.DataAnnotations;

namespace GorevNet.Models
{
    public enum TaskStatus
    {
        Beklemede,      
        DevamEdiyor,    
        Tamamlandı     
    }

    public enum TaskPriority
    {
        Düşük,          
        Normal,         
        Yüksek          
    }

    public class UserTask
    {

        public int Id { get; set; }

        [MaxLength(100)] public string Title { get; set; }

        [MaxLength(1500)] public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime? StatusChangedDate { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Comment { get; set; }
        public int AssignedUserId { get; set; }
        public int ManagerId { get; set; }

        //Sonradan Tag eklenebilir


    }
}
