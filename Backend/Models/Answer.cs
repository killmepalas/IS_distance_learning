using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Text { get; set; }
        
        [Required]
        public bool IsRight { get; set; }
        
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}