using System.ComponentModel.DataAnnotations;

namespace Footballers.DataProcessor.ImportDto
{
    public class ImportTeamsDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(40)]
        [RegularExpression(GlobalConstants.TeamNameRegex)]
        public string Name { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(40)]
        public string Nationality { get; set; }

        [Required]
        public int Trophies { get; set; }

        public int[] Footballers { get; set; }
    }
}
