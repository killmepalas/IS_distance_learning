using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class StudentAnswer
    {
        [Key]
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        public Account Student { get; set; }
        
        public int AnswerId { get; set; }
        public Answer Answer { get; set; }
    }
}