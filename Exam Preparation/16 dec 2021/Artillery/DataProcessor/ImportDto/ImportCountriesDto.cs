using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    [XmlType("Country")]
    public class ImportCountriesDto
    {
        [Required]
        [MinLength(4)]
        [MaxLength(60)]
        [XmlElement("CountryName")]
        public string CountryName { get; set; }

        [Required, Range(50_000, 10_000_000)]
        [XmlElement("ArmySize")]
        public int ArmySize { get; set; }
    }
}
