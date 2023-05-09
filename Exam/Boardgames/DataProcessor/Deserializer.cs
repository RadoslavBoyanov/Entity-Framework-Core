using System.Text;
using Boardgames.Data.Models;
using Boardgames.Data.Models.Enums;
using Boardgames.DataProcessor.ImportDto;
using Newtonsoft.Json;

namespace Boardgames.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;
    using Boardgames.Data;
   
    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCreator
            = "Successfully imported creator – {0} {1} with {2} boardgames.";

        private const string SuccessfullyImportedSeller
            = "Successfully imported seller - {0} with {1} boardgames.";

        public static string ImportCreators(BoardgamesContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Creators";
            ImportCreatorsDto[] creatorsDto = Deserialize<ImportCreatorsDto[]>(xmlString, rootName);

            List<Creator> creators = new List<Creator>();

            foreach (var cDto in creatorsDto)
            {
                if (!IsValid(cDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Creator creator = new Creator()
                {
                    FirstName = cDto.FirstName,
                    LastName = cDto.LastName
                };

                foreach (var boardgameDto in cDto.Boardgames)
                {
                    if (!IsValid(boardgameDto))
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    Boardgame boardgame = new Boardgame()
                    {
                        Name = boardgameDto.Name,
                        Rating = boardgameDto.Rating,
                        YearPublished = boardgameDto.YearPublished,
                        CategoryType = (CategoryType)boardgameDto.CategoryType,
                        Mechanics = boardgameDto.Mechanics
                    };

                    creator.Boardgames.Add(boardgame);
                }

                creators.Add(creator);
                output.AppendLine(string.Format(SuccessfullyImportedCreator, creator.FirstName, creator.LastName,
                    creator.Boardgames.Count));
            }

            context.Creators.AddRange(creators);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportSellers(BoardgamesContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            ImportSellerDto[] sellerDtos = JsonConvert.DeserializeObject<ImportSellerDto[]>(jsonString);

            List<Seller> sellers = new List<Seller>();

            foreach (var sDto in sellerDtos)
            {
                if (!IsValid(sDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Seller seller = new Seller()
                {
                    Name = sDto.Name,
                    Address = sDto.Address,
                    Country = sDto.Country,
                    Website = sDto.Website
                };

                foreach (var boardgame in sDto.Boardgames.Distinct())
                {
                    Boardgame bg = context.Boardgames.Find(boardgame);

                    if (bg == null)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    seller.BoardgamesSellers.Add(new BoardgameSeller()
                    {
                        Boardgame = bg
                    });
                }

                sellers.Add(seller);

                output.AppendLine(
                    string.Format(SuccessfullyImportedSeller, seller.Name, seller.BoardgamesSellers.Count));

            }

            context.Sellers.AddRange(sellers);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }

        //Helper method Xml Import
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
