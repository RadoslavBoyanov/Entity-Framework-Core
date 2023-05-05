using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using VaporStore.Data.Models;
using VaporStore.Data.Models.Enums;
using VaporStore.DataProcessor.ImportDto;

namespace VaporStore.DataProcessor
{
    using System.ComponentModel.DataAnnotations;

    using Data;

    public static class Deserializer
    {
        public const string ErrorMessage = "Invalid Data";

        public const string SuccessfullyImportedGame = "Added {0} ({1}) with {2} tags";

        public const string SuccessfullyImportedUser = "Imported {0} with {1} cards";

        public const string SuccessfullyImportedPurchase = "Imported {0} for {1}";

        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportGamesDto[] gameDtos = JsonConvert.DeserializeObject<ImportGamesDto[]>(jsonString);

            List<Game> games = new List<Game>();
            List<Developer> developers = new List<Developer>();
            List<Genre> genres = new List<Genre>();
            List<Tag> tags = new List<Tag>();

            foreach (ImportGamesDto gameDto in gameDtos)
            {
                if (!IsValid(gameDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime releaseDate;
                bool isReleaseDateValid = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

                if (!isReleaseDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (gameDto.Tags.Length == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Game g = new Game()
                {
                    Name = gameDto.Name,
                    Price = gameDto.Price,
                    ReleaseDate = releaseDate
                };

                Developer gameDev = developers
                    .FirstOrDefault(d => d.Name == gameDto.Developer);

                if (gameDev == null)
                {
                    Developer newGameDev = new Developer()
                    {
                        Name = gameDto.Developer
                    };
                    developers.Add(newGameDev);

                    g.Developer = newGameDev;
                }
                else
                {
                    g.Developer = gameDev;
                }

                Genre gameGenre = genres
                    .FirstOrDefault(g => g.Name == gameDto.Genre);

                if (gameGenre == null)
                {
                    Genre newGenre = new Genre()
                    {
                        Name = gameDto.Genre
                    };

                    genres.Add(newGenre);
                    g.Genre = newGenre;
                }
                else
                {
                    g.Genre = gameGenre;
                }

                foreach (string tagName in gameDto.Tags)
                {
                    if (String.IsNullOrEmpty(tagName))
                    {
                        continue;
                    }

                    Tag gameTag = tags
                        .FirstOrDefault(t => t.Name == tagName);

                    if (gameTag == null)
                    {
                        Tag newGameTag = new Tag()
                        {
                            Name = tagName
                        };

                        tags.Add(newGameTag);
                        g.GameTags.Add(new GameTag()
                        {
                            Game = g,
                            Tag = newGameTag
                        });
                    }
                    else
                    {
                        g.GameTags.Add(new GameTag()
                        {
                            Game = g,
                            Tag = gameTag
                        });
                    }
                }

                if (g.GameTags.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                games.Add(g);
                sb.AppendLine(String.Format(SuccessfullyImportedGame, g.Name, g.Genre.Name, g.GameTags.Count));
            }

            context.Games.AddRange(games);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportUsersDto[] userDtos = JsonConvert.DeserializeObject<ImportUsersDto[]>(jsonString);

            List<User> users = new List<User>();

            foreach (ImportUsersDto userDto in userDtos)
            {
                if (!IsValid(userDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                List<Card> userCards = new List<Card>();

                bool areAllCardsValid = true;
                foreach (ImportUserCardsDto cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        areAllCardsValid = false;
                        break;
                    }

                    Object cardTypeRes;
                    bool isCardTypeValid = Enum.TryParse(typeof(CardType), cardDto.Type, out cardTypeRes);

                    if (!isCardTypeValid)
                    {
                        areAllCardsValid = false;
                        break;
                    }

                    CardType cardType = (CardType)cardTypeRes;

                    userCards.Add(new Card()
                    {
                        Number = cardDto.Number,
                        Cvc = cardDto.Cvc,
                        Type = cardType
                    });
                }

                if (!areAllCardsValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (userCards.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                User u = new User()
                {
                    Username = userDto.Username,
                    FullName = userDto.FullName,
                    Email = userDto.Email,
                    Age = userDto.Age,
                    Cards = userCards
                };

                users.Add(u);
                sb.AppendLine(String.Format(SuccessfullyImportedUser, u.Username, u.Cards.Count));
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            return output.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}