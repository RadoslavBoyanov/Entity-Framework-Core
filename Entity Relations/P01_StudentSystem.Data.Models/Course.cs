using System.ComponentModel.DataAnnotations;

namespace P01_StudentSystem.Data.Models
{
    public class Course
    {
        public Course()
        {
            StudentsCourses = new HashSet<StudentCourse>();
            Homeworks = new HashSet<Homework>();
            Resources = new HashSet<Resource>();
        }

        [Key]
        public int CourseId { get; set; }
        [MaxLength(80)]
        [DataType("nvarchar")]
        [Required]
        public string Name { get; set; }

        [DataType("nvarchar")]
        public string? Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public decimal Price { get; set; }

        public ICollection<StudentCourse> StudentsCourses { get; set; }
        public ICollection<Homework> Homeworks { get; set; }
        public ICollection<Resource> Resources { get; set; }
    }
}