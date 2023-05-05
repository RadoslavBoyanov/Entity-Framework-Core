using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VaporStore.DataProcessor.ImportDto
{
    public class ImportUserCardsDto
    {
        [Required]
        [RegularExpression(@"^(\\d{4})\\s(\\d{4})\\s(\\d{4})\\s(\\d{4})$")]
        public string Number { get; set; }

        [Required]
        [MaxLength(3)]
        [RegularExpression(@"^(\\d{3})$")]
        public string Cvc { get; set; }

        public string Type { get; set; }
    }
}
