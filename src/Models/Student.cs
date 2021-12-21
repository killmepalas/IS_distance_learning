using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }

        public List<Attempt> Attempts { get; set; } = new List<Attempt>();
        public List<TestGrade> TestsGrades { get; set; } = new List<TestGrade>();
        public List<CourseGrade> CourseGrades { get; set; } = new List<CourseGrade>();
    }
}