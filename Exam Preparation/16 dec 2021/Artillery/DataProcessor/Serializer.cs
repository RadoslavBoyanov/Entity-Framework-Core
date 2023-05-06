
using Artillery.Data.Models.Enums;
using Artillery.DataProcessor.ExportDto;
using Newtonsoft.Json;

namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportShells(ArtilleryContext context, double shellWeight)
        {
            var shells = context.Shells
                .Where(s=> s.ShellWeight > shellWeight)
                .Select(s=> new
                {
                    ShellWeight = s.ShellWeight,
                    Caliber = s.Caliber,
                    Guns = s.Guns
                        .Where(g=> (int)g.GunType == 3)
                        
                        .Select(g=> new
                        {
                            GunType = g.GunType.ToString(),
                            GunWeight = g.GunWeight,
                            BarrelLength = g.BarrelLength,
                            Range = g.Range > 3000 ? "Long-range" : "Regular range"
                        }).OrderByDescending(x => x.GunWeight)
                        .ToArray()
                }).OrderBy(x=>x.ShellWeight)
                .ToArray();

            return JsonConvert.SerializeObject(shells, Formatting.Indented);
        }

        public static string ExportGuns(ArtilleryContext context, string manufacturer)
        {
            var guns = context.Guns
                .Where(x=>x.Manufacturer.ManufacturerName == manufacturer)
                .Select(g=> new ExportGunsDto()
                {
                    Manufacturer = g.Manufacturer.ManufacturerName,
                    GunType = g.GunType.ToString(),
                    GunWeight = g.GunWeight,
                    BarrelLength = g.BarrelLength,
                    Range = g.Range,
                    Countries = g.CountriesGuns
                        .Where(c=>c.Country.ArmySize > 4_500_000)
                        
                        .Select(c=>new ExportGunCountriesDto()
                        {
                            CountryName = c.Country.CountryName,
                            ArmySize = c.Country.ArmySize
                        })
                        .OrderBy(x => x.ArmySize)
                        .ToArray()
                }).OrderBy(x=>x.BarrelLength)
                .ToArray();

            string rootName = "Guns";
            return Serialize(guns, rootName);
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
