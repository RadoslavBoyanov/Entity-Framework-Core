using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public StudentSystemContext(DbContextOptions options) : base(options)
        {

        }

        public StudentSystemContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=DESKTOP-455JGR0\SQLEXPRESS01;Database=StudentSystem;Trusted_Connection=True;Integrated Security=True;");
            }
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Homework> Homeworks { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentCourse> StudentsCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.HasKey(pk => new { pk.StudentId, pk.CourseId });
            });

            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.HasOne(e => e.Student)
                    .WithMany(e => e.StudentsCourses)
                    .HasForeignKey(e => e.StudentId);

                entity.HasOne(e => e.Course)
                    .WithMany(e => e.StudentsCourses)
                    .HasForeignKey(e => e.CourseId);
            });

        }
    }
}