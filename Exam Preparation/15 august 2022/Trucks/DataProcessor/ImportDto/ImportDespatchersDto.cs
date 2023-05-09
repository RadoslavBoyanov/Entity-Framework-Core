using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Trucks.DataProcessor.ImportDto
{
    [XmlType("Despatcher")]
    public class ImportDespatchersDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(40)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Position")]
        public string Position { get; set; }

        [XmlArray("Trucks")]
        public ImportTrucksDespatcherDto[] Trucks { get; set; }
    }
}
