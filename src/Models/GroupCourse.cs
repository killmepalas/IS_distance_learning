using System.ComponentModel.DataAnnotations;

namespace IS_distance_learning.Models
{
    public class GroupCourse
    {
        [Key]
        public int Id { get; set; }
        
        public int GroupId { get; set; }
        public Group Group { get; set; }
        
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}