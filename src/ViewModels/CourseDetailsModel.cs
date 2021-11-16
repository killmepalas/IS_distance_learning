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
        public string Name { get; set; }
        public Account Account { get; set; }
        public List<Group> Groups { get; set; }
    }
}
