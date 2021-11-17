using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace IS_distance_learning.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public Admin Admin { get; set; }
        public Teacher Teacher { get; set; }
        public Student Student { get; set; }
    }
}
