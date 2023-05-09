using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Trucks.Data.Models.Enums;

namespace Trucks.DataProcessor.ExportDto
{
    [XmlType("Truck")]
    public class DespatcherTrucksDto
    {
        [XmlElement("RegistrationNumber")]
        public string RegistrationNumber { get; set; }
        [XmlElement("Make")]
        public string MakeType { get; set; }
    }
}
