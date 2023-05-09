namespace Boardgames.DataProcessor.ExportDto
{
    public class ExportSellerDto
    {
        public string Name { get; set; }

        public string Website { get; set; }

        public ExportSellerBoardgamesDto[] Boardgames { get; set; }
    }
}
