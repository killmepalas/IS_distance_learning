using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        public int AccountId { get; set; }
        public Account Account { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();
    }
}
