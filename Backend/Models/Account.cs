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
        [MaxLength(10)]
        public string Login { get; set; }
        
        [Required]
        [MaxLength(16)]
        public string Password { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string MiddleName { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }
        
        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public int? GroupId { get; set; }
        public Group Group { get; set; }
    }
}
