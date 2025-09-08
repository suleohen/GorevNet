using System.ComponentModel.DataAnnotations;

namespace GorevNet.Models.ViewModels
{
    public class EditEmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad gereklidir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Departman seçimi gereklidir.")]
        public string Department { get; set; }

        public string Position { get; set; }

        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; }

        // Dropdown listeleri için
        public List<string> Departments { get; set; } = new();
        public List<string> Roles { get; set; } = new();

        // Rol yönetimi için yeni property'ler
        public string CurrentRole { get; set; } = "Employee";
        public string NewRole { get; set; } = "Employee";
    }
}