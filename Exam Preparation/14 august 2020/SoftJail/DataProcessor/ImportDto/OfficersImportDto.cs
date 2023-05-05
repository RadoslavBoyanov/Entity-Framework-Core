using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class OfficersImportDto
    {
        [Required, StringLength(GlobalConstants.OfficerFullNameMaxLength, MinimumLength = GlobalConstants.OfficerFullNameMinLength)]
        [XmlElement("Name")]
        public string FullName { get; set; }

        [Required, Range(typeof(decimal), GlobalConstants.SalaryOfficerMin, GlobalConstants.SalaryOfficerMax)]
        [XmlElement("Money")]
        public decimal Salary { get; set; }

        [Required]
        [XmlElement("Position")]
        public string Position { get; set; }

        [Required]
        [XmlElement("Weapon")]
        public string Weapon { get; set; }

        [Required]
        [XmlElement("DepartmentId")]
        public int DepartmentId { get; set; }

        [XmlArray("Prisoners")]
        public OfficerPrisonerImportDto[] Prisoners { get; set; }
    }
}
