using System.ComponentModel.DataAnnotations;

namespace SoftJail.Data.Models
{
    public class Department
    {
        public Department()
        {
            this.Cells = new HashSet<Cell>();
        }

        [Key]
        public int Id { get; set; }

        [Required, StringLength(GlobalConstants.DepartmentMaxLength, MinimumLength = GlobalConstants.DepartmentMinLength)]
        public string Name { get; set; }

        public ICollection<Cell> Cells { get; set; }
    }
}
