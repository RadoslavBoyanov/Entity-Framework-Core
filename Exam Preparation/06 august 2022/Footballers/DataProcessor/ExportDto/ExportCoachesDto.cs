using System.Security.AccessControl;
using System.Xml.Serialization;

namespace Footballers.DataProcessor.ExportDto
{
    [XmlType("Coach")]
    public class ExportCoachesDto
    {
        [XmlAttribute("FootballersCount")]
        public int FootballersCount { get; set; }

        [XmlElement("CoachName")]
        public string CoachName { get; set; }

        [XmlArray("Footballers")]
        public ExportCoachFootballersDto[] Footballers { get; set; }
    }
}
