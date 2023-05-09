using Boardgames.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Boardgames.DataProcessor.ExportDto
{
    public class ExportSellerBoardgamesDto
    {
        public string Name { get; set; }

        public double Rating { get; set; }

        public string Mechanics { get; set; }


        public string Category { get; set; }
    }
}
