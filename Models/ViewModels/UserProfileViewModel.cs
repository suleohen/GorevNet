using GorevNet.Models;
using System;
using System.Collections.Generic;


namespace GorevNet.ViewModels
{
    public class UserProfileViewModel
    {
        // Temel personel bilgileri
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }

        // Görev istatistikleri
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OngoingTasks { get; set; }

        // Son görevler (liste halinde)
        public List<UserTask> RecentTasks { get; set; } = new List<UserTask>();
    }
}
