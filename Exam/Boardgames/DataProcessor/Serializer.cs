using Boardgames.DataProcessor.ExportDto;
using Newtonsoft.Json;

namespace Boardgames.DataProcessor
{
    using Boardgames.Data;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportCreatorsWithTheirBoardgames(BoardgamesContext context)
        {
            var creators = context.Creators
                .Where(c => c.Boardgames.Any())
                .OrderByDescending(c=>c.Boardgames.Count)
                .ThenBy(c=>c.FirstName)
                .Select(c => new ExportCreatorDto()
                {
                    BoardgamesCount = c.Boardgames.Count,
                    Name = string.Join(" ", c.FirstName, c.LastName),
                    Boardgames = c.Boardgames
                        .Select(b => new ExportCreatorBoardgamesDto()
                        {
                            Name = b.Name,
                            YearPublished = b.YearPublished
                        })
                        .OrderBy(x => x.Name)
                        .ToArray()
                })
                .ToArray();

            string rootName = "Creators";

            return Serialize(creators, rootName);
        }

        public static string ExportSellersWithMostBoardgames(BoardgamesContext context, int year, double rating)
        {
            var sellers = context.Sellers
                .Where(s => s.BoardgamesSellers.Any(x =>
                    x.Boardgame.YearPublished >= year && x.Boardgame.Rating <= rating))
                .Select(s => new ExportSellerDto()
                {
                    Name = s.Name,
                    Website = s.Website,
                    Boardgames = s.BoardgamesSellers
                        .Where(b => b.Boardgame.YearPublished >= year && b.Boardgame.Rating <= rating)
                        .OrderByDescending(x => x.Boardgame.Rating)
                        .ThenBy(x => x.Boardgame.Name)
                        .Select(bg => new ExportSellerBoardgamesDto()
                        {
                            Name = bg.Boardgame.Name,
                            Rating = bg.Boardgame.Rating,
                            Mechanics = bg.Boardgame.Mechanics,
                            Category = bg.Boardgame.CategoryType.ToString()
                        })
                        .ToArray()
                })
                .OrderByDescending(x => x.Boardgames.Length)
                .ThenBy(x => x.Name)
                .Take(5)
                .ToArray();

            return JsonConvert.SerializeObject(sellers, Formatting.Indented);
        }

        //Helper method Xml Export
        private static string Serialize<T>(T dto, string rootName)
        {
            StringBuilder builder = new StringBuilder();
            XmlRootAttribute xmlRoot = new XmlRootAttribute(rootName);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            using StringWriter writer = new StringWriter(builder);
            xmlSerializer.Serialize(writer, dto, namespaces);

            return builder.ToString().TrimEnd();
        }
    }
}