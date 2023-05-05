using System.ComponentModel.DataAnnotations;

namespace SoftJail.DataProcessor.ImportDto
{
    public class DepartmentImportDto
    {
        [Required, StringLength(GlobalConstants.DepartmentMaxLength, MinimumLength = GlobalConstants.DepartmentMinLength)]
        public string Name { get; set; }

        public DepartmentCellsImportDto[] Cells { get; set; }
    }
}
