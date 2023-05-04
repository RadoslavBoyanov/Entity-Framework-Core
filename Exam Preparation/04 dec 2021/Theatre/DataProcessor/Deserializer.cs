using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Theatre.Data.Models;
using Theatre.Data.Models.Enums;
using Theatre.DataProcessor.ImportDto;

namespace Theatre.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using Theatre.Data;
    using Data.Models;


    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";



        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Plays";

            PlaysDto[] playsDtos = Deserialize<PlaysDto[]>(xmlString, rootName);

            List<Play> plays = new List<Play>();
            var needDuration = new TimeSpan(1, 0, 0);
            var recommendGenres = new string[] { "Drama", "Comedy", "Romance", "Musical" };

            foreach (var pDto in playsDtos)
            {
                var currentTime = TimeSpan.ParseExact(pDto.Duration, "c", CultureInfo.InvariantCulture);

                if (!IsValid(pDto) || currentTime < needDuration || !recommendGenres.Contains(pDto.Genre))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Play play = new Play()
                {
                    Title = pDto.Title,
                    Duration = TimeSpan.ParseExact(pDto.Duration, "c", CultureInfo.InvariantCulture),
                    Rating = pDto.Rating,
                    Genre = (Genre)Enum.Parse(typeof(Genre), pDto.Genre),
                    Description = pDto.Description,
                    Screenwriter = pDto.Screenwriter
                };
                plays.Add(play);
                output.AppendLine(String.Format(SuccessfulImportPlay, play.Title, play.Genre, play.Rating));
            }

            context.Plays.AddRange(plays);
            context.SaveChanges();


            return output.ToString().TrimEnd();
        }

        public static string ImportCasts(TheatreContext context, string xmlString)
        {
           StringBuilder output = new StringBuilder();

           string rootName = "Casts";
           CastsDto[] castDtos = Deserialize<CastsDto[]>(xmlString, rootName);

           List<Cast> casts = new List<Cast>();

           foreach (var cDto in castDtos)
           {
               if (!IsValid(cDto))
               {
                   output.AppendLine(ErrorMessage);
                   continue;
               }

               Cast cast = new Cast()
               {
                   FullName = cDto.FullName,
                   IsMainCharacter = bool.Parse(cDto.IsMainCharacter),
                   PhoneNumber = cDto.PhoneNumber,
                   PlayId = cDto.PlayId
               };

               casts.Add(cast);
               var characterRole = cDto.IsMainCharacter == "true" ? "main" : "lesser";

               output.AppendLine(String.Format(SuccessfulImportActor, cDto.FullName, characterRole));
           }

           context.Casts.AddRange(casts);
           context.SaveChanges();

           return output.ToString().TrimEnd();
        }

        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            TheatresDto[] theatresDtos = JsonConvert.DeserializeObject<TheatresDto[]>(jsonString);

            List<Theatre> theatres = new List<Theatre>();

            foreach (var tDto in theatresDtos)
            {
                if (!IsValid(tDto))
                {
                    output.AppendLine(ErrorMessage); 
                    continue;
                }

                Theatre theatre = new Theatre()
                {
                    Name = tDto.Name,
                    NumberOfHalls = tDto.NumberOfHalls,
                    Director = tDto.Director,
                };
                var tickets = new List<Ticket>();

                foreach (var ticketDto in tDto.Tickets)
                {
                    if (!IsValid(ticketDto))
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    Ticket ticket = new Ticket()
                    {
                        Price = ticketDto.Price,
                        RowNumber = ticketDto.RowNumber,
                        Theatre = theatre,
                        PlayId = ticketDto.PlayId
                    };
                    tickets.Add(ticket);
                }
                
                theatre.Tickets = tickets;
                theatres.Add(theatre);
                var totalNumber = theatre.Tickets.Count().ToString();

                output.AppendLine(String.Format(SuccessfulImportTheatre, tDto.Name, totalNumber));
            }

            context.Theatres.AddRange(theatres);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
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
