using System.Globalization;
using Newtonsoft.Json;
using Theatre.DataProcessor.ExportDto;

namespace Theatre.DataProcessor
{

    using System;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models;

    public class Serializer
    {
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {
            var result = context.Theatres.Where(x => x.NumberOfHalls >= numbersOfHalls &&
                                                               x.Tickets.Count() >= 20)
                .Select(x => new
                {
                    Name = x.Name,
                    Halls = x.NumberOfHalls,
                    TotalIncome = x.Tickets.Where(x => x.RowNumber <= 5).Sum(x => x.Price),
                    Tickets = x.Tickets.Where(x => x.RowNumber <= 5).Select(t => new
                        {
                            Price = t.Price,
                            RowNumber = t.RowNumber
                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                })
                .OrderByDescending(h => h.Halls)
                .ThenBy(n => n.Name).ToArray();


            string json = JsonConvert.SerializeObject(result, Formatting.Indented);

            return json;

        }

        public static string ExportPlays(TheatreContext context, double raiting)
        {
            var plays = context.Plays.Where(x => x.Rating <= raiting)
                .OrderBy(x => x.Title)
                .ThenByDescending(x => x.Genre)
                .Select(x => new ExportPlaysDto
                {
                    Title = x.Title,
                    Duration = x.Duration.ToString("c"),
                    Rating = x.Rating == 0 ? "Premier" : x.Rating.ToString(),
                    Genre = x.Genre.ToString(),
                    Actors = x.Casts.Where(x => x.IsMainCharacter)
                        .Select(a => new ActorsDto()
                        {
                            FullName = a.FullName,
                            MainCharacter = $"Plays main character in '{x.Title}'."
                        })
                        .OrderByDescending(x => x.FullName)
                        .ToArray()
                }).ToArray();

            string rootName = "Plays";

            return Serialize(plays, rootName);
        }


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
