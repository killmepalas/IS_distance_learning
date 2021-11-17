using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        
        public int TestId { get; set; }
        public Test Test { get; set; }
    }
}