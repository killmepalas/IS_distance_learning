using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public DateTime ExpirationDate { get; set; }

        public List<Question> Questions { get; set; } = new List<Question>();

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
