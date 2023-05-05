using System.ComponentModel.DataAnnotations;

namespace SoftJail.DataProcessor.ImportDto
{
    public class PrisonersImportDto
    {
        [Required, StringLength(GlobalConstants.FullNameMaxLengthPrisioner, MinimumLength = GlobalConstants.FullNameMinLengthPrisioner)]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(GlobalConstants.NickNameExpression)]
        public string Nickname { get; set; }

        [Required, Range(GlobalConstants.MinAgePrisioner, GlobalConstants.MaxAgePrisioner)]
        public int Age { get; set; }

        [Required]
        public string IncarcerationDate { get; set; }

        public string? ReleaseDate { get; set; }

        [Range(typeof(decimal), GlobalConstants.PrisonerBailMinValue, GlobalConstants.PrisonerBailMaxValue)]
        public decimal? Bail { get; set; }

        public int? CellId { get; set; }

        public MailPrisonerImportDto[] Mails { get; set; }
    }
}
