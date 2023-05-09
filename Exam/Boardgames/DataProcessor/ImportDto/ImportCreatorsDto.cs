using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Boardgames.DataProcessor.ImportDto
{
    [XmlType("Creator")]
    public class ImportCreatorsDto
    {
        [Required, StringLength(7, MinimumLength = 2)]
        [XmlElement("FirstName")]
        public string FirstName { get; set; }

        [Required, StringLength(7, MinimumLength = 2)]
        [XmlElement("LastName")]
        public string LastName { get; set; }

        [XmlArray("Boardgames")]
        public ImportCreatorBoardgamesDto[] Boardgames { get; set; }
    }
}
