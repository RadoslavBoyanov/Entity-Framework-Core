using System.Globalization;
using System.Text;
using Footballers.Data.Models;
using Footballers.Data.Models.Enums;
using Footballers.DataProcessor.ImportDto;
using Newtonsoft.Json;

namespace Footballers.DataProcessor
{
    using Footballers.Data;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCoach
            = "Successfully imported coach - {0} with {1} footballers.";

        private const string SuccessfullyImportedTeam
            = "Successfully imported team - {0} with {1} footballers.";

        public static string ImportCoaches(FootballersContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Coaches";
            ImportCoachesDto[] coachesDtos = Deserialize<ImportCoachesDto[]>(xmlString, rootName);

            List<Coach> coachList = new List<Coach>();

            foreach (var cDto in coachesDtos)
            {
                if (!IsValid(cDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Coach coach = new Coach()
                {
                    Name = cDto.Name,
                    Nationality = cDto.Nationality
                };

                foreach (var fDto in cDto.Footballers)
                {
                    if (!IsValid(fDto))
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime contractStart;
                    bool isValidStartDate = DateTime.TryParseExact(fDto.ContractStartDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out contractStart);

                    if (!isValidStartDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime contractEnd;
                    bool isValidEndDate = DateTime.TryParseExact(fDto.ContractEndDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out contractEnd);

                    if (!isValidEndDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (contractStart > contractEnd)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    Footballer footballer = new Footballer()
                    {
                        Name = fDto.Name,
                        ContractStartDate = contractStart,
                        ContractEndDate = contractEnd,
                        BestSkillType = (BestSkillType)fDto.BestSkillType,
                        PositionType = (PositionType)fDto.PositionType,
                    };
                    coach.Footballers.Add(footballer);
                }
                coachList.Add(coach);
                output.AppendLine(string.Format(SuccessfullyImportedCoach, coach.Name, coach.Footballers.Count));
            }

            context.Coaches.AddRange(coachList);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportTeams(FootballersContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            ImportTeamsDto[] teamsDtos = JsonConvert.DeserializeObject<ImportTeamsDto[]>(jsonString);

            List<Team> teams = new List<Team>();

            foreach (var tDto in teamsDtos)
            {
                if (!IsValid(tDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                if (tDto.Trophies == 0)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Team team = new Team()
                {
                    Name = tDto.Name,
                    Nationality = tDto.Nationality
                };

                foreach (var footballerId in tDto.Footballers.Distinct())
                {
                    var spotplayer = context.Footballers.Find(footballerId);

                    if (spotplayer == null)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    TeamFootballer footballer = new TeamFootballer()
                    {
                        Footballer = spotplayer
                    };
                    team.TeamsFootballers.Add(footballer);
                }
                teams.Add(team);
                output.AppendLine(string.Format(SuccessfullyImportedTeam, team.Name, team.TeamsFootballers.Count));
            }

            context.Teams.AddRange(teams);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }

        //Helper method Import
        private static T Deserialize<T>(string inputXml, string rootName)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute(rootName);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            using StringReader reader = new StringReader(inputXml);

            T dtos = (T)xmlSerializer.Deserialize(reader);

            return dtos;
        }
    }
}
