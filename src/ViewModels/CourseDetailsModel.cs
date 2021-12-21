using IS_distance_learning.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.ViewModels
{
    [Keyless]
    public class CourseDetailsModel
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Teacher Teacher { get; set; }
        public List<Test> Tests { get; set; }
        public List<Group> Groups { get; set; }
        public List<Attempt> Attempts { get; set; }
        public List<TestGrade> TestsGrades { get; set; }
        public int? CourseGrade { get; set; }
    }
}
