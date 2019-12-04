using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.Data.Models
{    
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(3), MaxLength(25)]
        public string Name { get; set; }

        public IEnumerable<Cell> Cells { get; set; } = new HashSet<Cell>();
    }
}
