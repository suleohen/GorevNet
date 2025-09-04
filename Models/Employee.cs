using System.ComponentModel.DataAnnotations;

namespace GorevNet.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required(ErrorMessage = "Ad gereklidir.")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string Position { get; set; }

        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool MustChangePassword { get; set; } = false;

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation property
        public virtual ICollection<UserTask> AssignedTasks { get; set; } = new List<UserTask>();

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}