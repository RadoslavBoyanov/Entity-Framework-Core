using Newtonsoft.Json;
using Trucks.Data.Models.Enums;
using Trucks.DataProcessor.ExportDto;

namespace Trucks.DataProcessor
{
    using Data;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportDespatchersWithTheirTrucks(TrucksContext context)
        {
            var despatchers = context.Despatchers.Where(x=>x.Trucks.Any())
                .Select(d=>new DespatcherExportDto()
                {
                    Name = d.Name,
                    TrucksCount = d.Trucks.Count(),
                    Trucks = d.Trucks.Select(t=> new DespatcherTrucksDto()
                    {
                        RegistrationNumber = t.RegistrationNumber,
                        MakeType = t.MakeType.ToString(),
                    }).OrderBy(x=>x.RegistrationNumber)
                        .ToArray()
                }).OrderByDescending(x=>x.TrucksCount)
                .ThenBy(x=>x.Name)
                .ToArray();

            string rootName = "Despatchers";
            return Serialize(despatchers, rootName);
        }

        public static string ExportClientsWithMostTrucks(TrucksContext context, int capacity)
        {
            var clients = context.Clients.Where(c => c.ClientsTrucks.Any(t => t.Truck.TankCapacity >= capacity))
                .Select(c => new
                {
                    Name = c.Name,
                    Trucks = c.ClientsTrucks.Where(t => t.Truck.TankCapacity >= capacity)
                        .Select(t => new
                        {
                            TruckRegistrationNumber = t.Truck.RegistrationNumber,
                            VinNumber = t.Truck.VinNumber,
                            TankCapacity = t.Truck.TankCapacity,
                            CargoCapacity = t.Truck.CargoCapacity,
                            CategoryType = t.Truck.CategoryType.ToString(),
                            MakeType = t.Truck.MakeType.ToString(),
                        }).OrderBy(x => x.MakeType)
                        .ThenByDescending(x => x.CargoCapacity)
                        .ToArray()
                }).OrderByDescending(x => x.Trucks.Length)
                .ThenBy(x => x.Name)
                .Take(10)
                .ToArray();

            return JsonConvert.SerializeObject(clients, Formatting.Indented);
        }
        
        //Helper method Export
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
