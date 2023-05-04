using System.ComponentModel.DataAnnotations;

namespace Theatre.DataProcessor.ImportDto
{
    public class TheatresDto
    {
        [Required]
        [StringLength(30, MinimumLength = 4)]
        public string Name { get; set; }

        [Required]
        [Range(typeof(sbyte), "1", "10")]
        public sbyte NumberOfHalls { get; set; }


        [StringLength(30, MinimumLength = 4)]
        public string Director { get; set; }

        public TheatresAndTicketsDto[] Tickets { get; set; }
    }
}
