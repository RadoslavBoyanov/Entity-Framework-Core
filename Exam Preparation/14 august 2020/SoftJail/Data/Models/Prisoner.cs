using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftJail.Data.Models
{
    public class Prisoner
    {
        public Prisoner()
        {
            this.Mails = new HashSet<Mail>();
            this.PrisonerOfficers = new HashSet<OfficerPrisoner>();
        }

        [Key]
        public int Id { get; set; }

        [Required, StringLength(GlobalConstants.FullNameMaxLengthPrisioner, MinimumLength = GlobalConstants.FullNameMinLengthPrisioner)]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(GlobalConstants.NickNameExpression)]
        public string Nickname { get; set; }

        [Required, Range(GlobalConstants.MinAgePrisioner, GlobalConstants.MaxAgePrisioner)]
        public int Age { get; set; }

        [Required]
        public DateTime IncarcerationDate { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public decimal? Bail { get; set; }

        [ForeignKey(nameof(Cell))]
        public int? CellId { get; set; }
        public Cell Cell { get; set; }

        public ICollection<Mail> Mails { get; set; }

        public ICollection<OfficerPrisoner> PrisonerOfficers { get; set; }
    }
}
