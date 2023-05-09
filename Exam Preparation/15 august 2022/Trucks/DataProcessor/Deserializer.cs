using System.Text;
using Newtonsoft.Json;
using Trucks.Data.Models;
using Trucks.Data.Models.Enums;
using Trucks.DataProcessor.ImportDto;

namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;
    using Data;


    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Despatchers";
            ImportDespatchersDto[] despatchersDtos = Deserialize<ImportDespatchersDto[]>(xmlString, rootName);

            List<Despatcher> despatchers = new List<Despatcher>();

            foreach (var dDto in despatchersDtos)
            {
                if (!IsValid(dDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                string position = dDto.Position;
                bool isPositionInvalid = string.IsNullOrEmpty(position);

                if (isPositionInvalid)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Despatcher d = new Despatcher()
                {
                    Name = dDto.Name,
                    Position = position
                };

                foreach (var truckDto in dDto.Trucks)
                {
                    if (!IsValid(truckDto))
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    Truck t = new Truck()
                    {
                        RegistrationNumber = truckDto.RegistrationNumber,
                        VinNumber = truckDto.VinNumber,
                        TankCapacity = truckDto.TankCapacity,
                        CargoCapacity = truckDto.CargoCapacity,
                        CategoryType = (CategoryType)truckDto.CategoryType,
                        MakeType = (MakeType)truckDto.MakeType
                    };

                    d.Trucks.Add(t);
                }
                despatchers.Add(d);
                output.AppendLine(String.Format(SuccessfullyImportedDespatcher, d.Name, d.Trucks.Count));
            }

            context.Despatchers.AddRange(despatchers);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }
        public static string ImportClient(TrucksContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            ImportClientsDto[] clientsDtos = JsonConvert.DeserializeObject<ImportClientsDto[]>(jsonString);

            List<Client> clients = new List<Client>();

            foreach (var cDto in clientsDtos)
            {
                if (!IsValid(cDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Client client = new Client()
                {
                    Name = cDto.Name,
                    Nationality = cDto.Nationality,
                    Type = cDto.Type
                };

                if (cDto.Type == "usual")
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                foreach (var truckId in cDto.Trucks.Distinct())
                {
                    Truck truck = context.Trucks.Find(truckId);

                    if (truck == null)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    client.ClientsTrucks.Add(new ClientTruck()
                    {
                        Truck = truck
                    });
                }

                clients.Add(client);
                output.AppendLine(string.Format(SuccessfullyImportedClient, client.Name, client.ClientsTrucks.Count));
            }

            context.Clients.AddRange(clients);
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