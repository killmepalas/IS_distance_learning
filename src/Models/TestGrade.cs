using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.Models
{
    public class TestGrade
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int? TestId { get; set; }
        public Test Test { get; set; }

        public int Grade { get; set; }
    }
}
