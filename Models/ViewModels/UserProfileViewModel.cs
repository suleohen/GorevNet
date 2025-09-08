using System.ComponentModel.DataAnnotations;

namespace GorevNet.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad gereklidir.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Departman")]
        public string Department { get; set; }

        [Display(Name = "Pozisyon")]
        public string Position { get; set; }

        [Display(Name = "İşe Giriş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        [Display(Name = "Durum")]
        public bool IsActive { get; set; }

        // İstatistik bilgileri (readonly)
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OngoingTasks { get; set; }

        // Son görevler
        public List<UserTask> RecentTasks { get; set; } = new List<UserTask>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        public double CompletionRate => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0;
    }
}