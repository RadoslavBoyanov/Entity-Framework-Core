using System.Xml.Serialization;

namespace Trucks.DataProcessor.ExportDto
{
    [XmlType("Despatcher")]
    public class DespatcherExportDto
    {
        [XmlAttribute("TrucksCount")]
        public int TrucksCount { get; set; }

        [XmlElement("DespatcherName")]
        public string Name { get; set; }

        [XmlArray("Trucks")]
        public DespatcherTrucksDto[] Trucks { get; set; }
    }
}
