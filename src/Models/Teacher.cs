using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Teacher : Account
    {
        [Key]
        public int Id { get; set; }
    }
}