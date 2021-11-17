using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }
    }
}