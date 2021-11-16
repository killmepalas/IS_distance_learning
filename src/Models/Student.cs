using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Student : Account
    {
        [Key]
        public int Id { get; set; }
        
        public int? GroupId { get; set; }
        public Group Group { get; set; }
    }
}