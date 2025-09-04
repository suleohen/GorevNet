using System.ComponentModel.DataAnnotations;

namespace GorevNet.Models.ViewModels
{
    public class CreateEmployeeViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Department { get; set; }

        public string Position { get; set; }

        public DateTime HireDate { get; set; }

        [Required]
        public string Role { get; set; } // Employee / Manager / Admin

        public List<string> Departments { get; set; } = new();
        public List<string> Roles { get; set; } = new();

    }

}
