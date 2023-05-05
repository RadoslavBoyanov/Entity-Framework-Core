using System.ComponentModel.DataAnnotations;

namespace SoftJail.DataProcessor.ImportDto
{
    public class DepartmentCellsImportDto
    {
        [Required, Range(GlobalConstants.CellNumberMin, GlobalConstants.CellNumberMax)]
        public int CellNumber { get; set; }

        [Required]
        public bool HasWindow { get; set; }
    }
}
