using System.ComponentModel.DataAnnotations;

namespace GorevNet.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        [MaxLength(100)]
        public string Email { get; set; }


        [MaxLength(50)]
        public string Department { get; set; }

        public DateTime HireDate { get; set; }

    }
}
