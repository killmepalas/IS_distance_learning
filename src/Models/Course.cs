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
        public string Name { get; set; }
        public string Description { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Test> Tests { get; set; } = new List<Test>();
        public List<CourseGrade> CourseGrades { get; set; } = new List<CourseGrade>();
    }
}
