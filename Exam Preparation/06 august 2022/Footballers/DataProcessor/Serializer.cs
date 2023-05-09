using System.Globalization;
using Footballers.DataProcessor.ExportDto;
using Newtonsoft.Json;

namespace Footballers.DataProcessor
{
    using Data;
    using Footballers.Data.Models.Enums;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportCoachesWithTheirFootballers(FootballersContext context)
        {
            var coaches = context.Coaches
                .Where(c => c.Footballers.Any())
                .Select(c => new ExportCoachesDto()
                {
                    FootballersCount = c.Footballers.Count(),
                    CoachName = c.Name,
                    Footballers = c.Footballers
                        .Select(f => new ExportCoachFootballersDto()
                        {
                            Name = f.Name,
                            Position = f.PositionType.ToString()
                        })
                        .OrderBy(x => x.Name)
                        .ToArray()
                })
                .OrderByDescending(x => x.FootballersCount)
                .ThenBy(x => x.CoachName)
                .ToArray();

            string rootName = "Coaches";
            return Serialize(coaches, rootName);
        }

        public static string ExportTeamsWithMostFootballers(FootballersContext context, DateTime date)
        {
            var teams = context.Teams
                .Where(t => t.TeamsFootballers.Any(tf => tf.Footballer.ContractStartDate >= date))
                .Select(t => new
                {
                    Name = t.Name,
                    Footballers = t.TeamsFootballers
                        .Where(tf => tf.Footballer.ContractStartDate >= date)
                        .OrderByDescending(x => x.Footballer.ContractEndDate)
                        .ThenBy(x => x.Footballer.Name)
                        .Select(tf => new
                        {
                            FootballerName = tf.Footballer.Name,
                            ContractStartDate = tf.Footballer.ContractStartDate.ToString("d", CultureInfo.InvariantCulture),
                            ContractEndDate = tf.Footballer.ContractEndDate.ToString("d", CultureInfo.InvariantCulture),
                            BestSkillType = tf.Footballer.BestSkillType.ToString(),
                            PositionType = tf.Footballer.PositionType.ToString(),
                        })
                        .ToArray()
                }).OrderByDescending(x => x.Footballers.Length)
                .ThenBy(x => x.Name)
                .Take(5)
                .ToArray();

            return JsonConvert.SerializeObject(teams, Formatting.Indented);
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
