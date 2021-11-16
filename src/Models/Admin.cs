using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class Admin : Account
    {
        [Key]
        public int Id { get; set; }
    }
}