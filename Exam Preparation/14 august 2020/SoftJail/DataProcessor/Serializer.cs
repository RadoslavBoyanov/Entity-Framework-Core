using Newtonsoft.Json;
using SoftJail.DataProcessor.ExportDto;

namespace SoftJail.DataProcessor
{
    using Data;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners.Where(p => ids.Contains(p.Id))
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.FullName,
                    CellNumber = p.Cell.CellNumber,
                    Officers = p.PrisonerOfficers.Select(o => new
                    {
                        OfficerName = o.Officer.FullName,
                        Department = o.Officer.Department.Name
                    }).OrderBy(x => x.OfficerName).ToArray(),
                    TotalOfficerSalary = Math.Round(p.PrisonerOfficers.Sum(po => po.Officer.Salary), 2)
                }).OrderBy(x => x.Name)
                .ThenBy(x => x.Id).ToArray();

            return JsonConvert.SerializeObject(prisoners, Formatting.Indented);
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var prisoners = context.Prisoners.Where(p=> prisonersNames.Contains(p.FullName)).Select(p=> new ExportPrisonersDto()
            {
                Id = p.Id,
                FullName = p.FullName,
                IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd"),
                EncryptedMessages = p.Mails.Select(m=> new EncryptedMessagesDto()
                {
                    Description = String.Join("", m.Description.Reverse())
                }).ToArray()
            }).OrderBy(x=>x.FullName)
                .ThenBy(x=>x.Id)
                .ToArray();

            string rootName = "Prisoners";
            return Serialize(prisoners, rootName);
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