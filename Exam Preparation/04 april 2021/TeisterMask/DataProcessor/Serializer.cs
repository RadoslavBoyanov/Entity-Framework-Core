using System.Globalization;
using Newtonsoft.Json;
using TeisterMask.Data.Models.Enums;
using TeisterMask.DataProcessor.ExportDto;

namespace TeisterMask.DataProcessor
{
    using Data;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var projects = context.Projects.Where(p => p.Tasks.Any())
                .Select(p => new ProjectsDto()
                {
                    Name = p.Name,
                    HasEndDate = p.DueDate.HasValue ? "Yes" : "No",
                    TasksCount = p.Tasks.Count(),
                    Tasks = p.Tasks.Select(t=>new TasksDto()
                    {
                        Name = t.Name,
                        Label = t.LabelType.ToString(),
                    }).OrderBy(x=>x.Name).ToArray()
                }).OrderByDescending(x=>x.TasksCount)
                .ThenBy(x=>x.Name).ToArray();

            string rootName = "Projects";
            return Serialize(projects, rootName);
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .Where(e => e.EmployeesTasks.Any(et => et.Task.OpenDate >= date)).Select(e => new
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks.Where(t => t.Task.OpenDate >= date).OrderByDescending(t => t.Task.DueDate)
                        .ThenBy(t => t.Task.Name)
                        .Select(t => new
                        {
                            TaskName = t.Task.Name,
                            OpenDate = t.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                            DueDate = t.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                            LabelType = t.Task.LabelType.ToString(),
                            ExecutionType = t.Task.ExecutionType.ToString(),
                        }).ToArray()
                }).OrderByDescending(e => e.Tasks.Length)
                .ThenBy(e => e.Username).Take(10).ToArray();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
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
