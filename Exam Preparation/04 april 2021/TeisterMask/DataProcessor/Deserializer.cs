// ReSharper disable InconsistentNaming

using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using TeisterMask.Data.Models;
using TeisterMask.Data.Models.Enums;
using TeisterMask.DataProcessor.ImportDto;
using Task = TeisterMask.Data.Models.Task;

namespace TeisterMask.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Projects";
            ProjectsImportDto[] projectsDtos = Deserialize<ProjectsImportDto[]>(xmlString, rootName);

            List<Project> projects = new List<Project>();

            foreach (var pDto in projectsDtos)
            {
                if (!IsValid(pDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime openDateTime;
                bool isValidOpen = DateTime.TryParseExact(pDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out openDateTime);

                if (!isValidOpen)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime? dueDate = null;

                if (!String.IsNullOrWhiteSpace(pDto.DueDate))
                {
                    DateTime dueDateDto;
                    bool isValidDueDate = DateTime.TryParseExact(pDto.DueDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out
                    dueDateDto);

                    if (!isValidDueDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    dueDate = dueDateDto;
                }

                Project project = new Project()
                {
                    Name = pDto.Name,
                    OpenDate = openDateTime,
                    DueDate = dueDate
                };


                foreach (var taskDto in pDto.Tasks)
                {
                    if (!IsValid(taskDto))
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime taskOpenDate;
                    bool isValidTaskOpenDate = DateTime.TryParseExact(taskDto.OpenDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out taskOpenDate);

                    if (!isValidTaskOpenDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime taskDueDate;
                    bool isValidTaskDueDate = DateTime.TryParseExact(taskDto.DueDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out taskDueDate);
                    if (!isValidTaskDueDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (taskOpenDate < openDateTime)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (dueDate.HasValue && taskDueDate > dueDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    Task task = new Task()
                    {
                        Name = taskDto.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = (ExecutionType)taskDto.ExecutionType,
                        LabelType = (LabelType)taskDto.LabelType,
                    };

                    project.Tasks.Add(task);
                }

                projects.Add(project);
                output.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));
            }

            context.Projects.AddRange(projects);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            EmployeesImportDto[] employeesDtos = JsonConvert.DeserializeObject<EmployeesImportDto[]>(jsonString);

            List<Employee> employees = new List<Employee>();

            foreach (var eDto in employeesDtos)
            {
                if (!IsValid(eDto))
                {
                    output.Append(ErrorMessage); continue;
                }

                Employee employee = new Employee()
                {
                    Username = eDto.Username,
                    Email = eDto.Email,
                    Phone = eDto.Phone
                };

                foreach (int taskId in eDto.Tasks.Distinct())
                {
                    Task tId = context.Tasks.Find(taskId);

                    if (tId == null)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    employee.EmployeesTasks.Add(new EmployeeTask()
                    {
                        Task = tId
                    });
                }
                employees.Add(employee);
                output.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username,
                    employee.EmployeesTasks.Count));
            }

            context.Employees.AddRange(employees);
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