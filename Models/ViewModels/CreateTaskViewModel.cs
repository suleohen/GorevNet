using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GorevNet.Models;

namespace GorevNet.ViewModels
{
    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "Görev başlığı zorunludur.")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Görev açıklaması zorunludur.")]
        [MaxLength(1500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Öncelik seçilmelidir.")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Bir çalışan seçmelisiniz.")]
        public int AssignedUserId { get; set; }

        // dropdown için kullanılacak çalışan listesi
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();

        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}
