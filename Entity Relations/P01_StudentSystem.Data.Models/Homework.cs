using System.ComponentModel.DataAnnotations;

namespace P01_StudentSystem.Data.Models
{
    public class Homework
    {
        public enum ContentTypes { Application, Pdf, Zip }
        public int HomeworkId { get; set; }
        [DataType("varchar")] [Required] public string Content { get; set; } = null!;
        public ContentTypes ContentType { get; set; }
        public DateTime SubmissionTime { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
