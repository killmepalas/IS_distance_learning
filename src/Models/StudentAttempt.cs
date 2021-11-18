using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.Models
{
    public class StudentAttempt
    {
        [Key]
        public int Id { get; set; }
                
        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int TestId { get; set; }
        public Test Test { get; set; }
    }
}
