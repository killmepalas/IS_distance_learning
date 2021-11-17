using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }

        public List<Course> Courses { get; set; } = new List<Course>();
    }
}