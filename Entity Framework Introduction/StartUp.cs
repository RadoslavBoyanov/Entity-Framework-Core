using System.Text;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SoftUni.Data;
using SoftUni.Models;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext dbContext = new SoftUniContext();

            //Console.WriteLine(GetEmployeesFullInformation(dbContext));
            //Console.WriteLine(GetEmployeesWithSalaryOver50000(dbContext));
            //Console.WriteLine(GetEmployeesFromResearchAndDevelopment(dbContext));
            //Console.WriteLine(AddNewAddressToEmployee(dbContext));
            //Console.WriteLine(GetEmployeesInPeriod(dbContext));
            //Console.WriteLine(GetAddressesByTown(dbContext));
            //Console.WriteLine(GetEmployee147(dbContext));
            //Console.WriteLine(GetDepartmentsWithMoreThan5Employees(dbContext));
            //Console.WriteLine(GetLatestProjects(dbContext));
            //Console.WriteLine(IncreaseSalaries(dbContext));
            //Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(dbContext));
            //Console.WriteLine(DeleteProjectById(dbContext));
            Console.WriteLine(RemoveTown(dbContext));
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder output  = new StringBuilder();
            var employees = context.Employees.
                OrderBy(e=>e.EmployeeId)
                .Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.MiddleName,
                e.JobTitle,
                e.Salary
            }).ToArray();

            foreach (var employee in employees)
            {
                output.AppendLine(
                    $"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:F2}");
            }

            return output.ToString().TrimEnd();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder();
            var employees = context.Employees.
                Select(e => new
                {
                    e.FirstName,
                    e.Salary
                }).Where(s => s.Salary > 50000)
                .OrderBy(e => e.FirstName).ToArray();

            foreach (var e in employees)
            {
                output.AppendLine($"{e.FirstName} - {e.Salary:F2}");
            }

            return output.ToString().TrimEnd();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder();

            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.Department,
                    x.Salary
                })
                .OrderBy(x => x.Salary).ThenByDescending(x => x.FirstName).ToArray();


            foreach (var employee in employees)
            {
                output.AppendLine(
                    $"{employee.FirstName} {employee.LastName} from {employee.Department.Name} - ${employee.Salary:F2}");
            }

            return output.ToString().TrimEnd();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAdress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            context.Addresses.Add(newAdress);

            Employee nakov = context.Employees.First(e => e.LastName == "Nakov");

            nakov.Address = newAdress;

            context.SaveChanges();

            string[] addressTexts = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e=>e.Address.AddressText).ToArray();

            StringBuilder output = new StringBuilder();

            foreach (var address in addressTexts)
            {
                output.AppendLine(address);
            }

            return output.ToString().TrimEnd();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.EmployeesProjects.Any(p =>
                    p.Project.StartDate.Year >= 2001 && p.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(x => new {
                    x.FirstName,
                    x.LastName,
                    x.Manager,
                    Projects = x.EmployeesProjects.Select(p => new
                    {
                        p.Project.Name,
                        StartDate = p.Project.StartDate.ToString("M'/'d'/'yyyy h:mm:ss tt"),
                        EndDate = p.Project.EndDate.HasValue ? p.Project.EndDate.Value.ToString("M'/'d'/'yyyy h:mm:ss tt") : "not finished"
                    })
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var em in employees)
            {
                sb.AppendLine($"{em.FirstName} {em.LastName} - Manager: {em.Manager.FirstName} {em.Manager.LastName}");
                foreach (var p in em.Projects)
                    sb.AppendLine($"--{p.Name} - {p.StartDate} - {p.EndDate}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Select(e => new
                {
                    Address = e.AddressText,
                    Town = e.Town.Name,
                    Employees = e.Employees.Count
                })
                .OrderByDescending(e => e.Employees)
                .ThenBy(e => e.Town)
                .ThenBy(e => e.Address)
                .Take(10)
                .ToArray();

            StringBuilder output = new StringBuilder();

            foreach (var e in addresses)
            {
                output.AppendLine($"{e.Address}, {e.Town} - {e.Employees} employees");
            }

            return output.ToString().TrimEnd();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees.Include(e => e.EmployeesProjects).ThenInclude(ep => ep.Project)
                .FirstOrDefault(e => e.EmployeeId == 147);

            StringBuilder output = new StringBuilder();
            output.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            foreach (var ep in employee.EmployeesProjects.OrderBy(x => x.Project.Name))
                output.AppendLine(ep.Project.Name);

            return output.ToString().TrimEnd();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .Select(d => new
                {
                    DepartmentName = d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
                    Employees = d.Employees
                })
                .OrderBy(em => em.Employees.Count)
                .ThenBy(m => m.DepartmentName)
                .ToArray();

            StringBuilder output = new StringBuilder();

            foreach (var d in departments)
            {
                output.AppendLine($"{d.DepartmentName} - {d.ManagerFirstName}  {d.ManagerLastName} ");

                foreach (var e in d.Employees.OrderBy(x=>x.FirstName).ThenBy(x=>x.LastName))
                {
                    output.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
                }
            }

            return output.ToString().TrimEnd();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .OrderByDescending(x => x.StartDate)
                .Take(10)
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    StartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt")
                })
                .OrderBy(p => p.Name)
                .ToArray();

            StringBuilder output = new StringBuilder();

            foreach (var project in projects)
            {
                output.AppendLine(project.Name);
                output.AppendLine(project.Description);
                output.AppendLine(project.StartDate);
            }

            return output.ToString().TrimEnd();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            HashSet<string> departments = new HashSet<string>()
            {
                "Engineering",
                "Tool Design", 
                "Marketing",
                "Information Services"
            };

            var employees = context.Employees
                .Where(x => departments.Contains(x.Department.Name))
                .OrderBy(e=> e.FirstName)
                .ThenBy(e=> e.LastName)
                .ToList();

            foreach (var employee in employees)
            {
                employee.Salary *= 1.12m;
            }

            context.SaveChanges();

            StringBuilder output = new StringBuilder();

            foreach (var employee in employees)
            {
                output.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:F2})");
            }

            return output.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e=>e.FirstName)
                .ThenBy(e=>e.LastName)
                .ToArray();

            StringBuilder output = new StringBuilder();

            foreach (var employee in employees)
            {
                output.AppendLine(
                    $"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:F2})");
            }

            return output.ToString().TrimEnd();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectsForDelete = context.Projects.First(x => x.ProjectId == 2);

            var emlpoyeeProjectForDelete = context.EmployeesProjects.Where(ep => ep.ProjectId == 2).ToArray();

            foreach (var employee in emlpoyeeProjectForDelete)
                context.EmployeesProjects.Remove(employee);

                context.Projects.Remove(projectsForDelete);

            context.SaveChanges();

            StringBuilder output = new StringBuilder();

            foreach (var project in context.Projects.Take(10))
                output.AppendLine(project.Name);

            return output.ToString().TrimEnd();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var townToRemove = context.Towns.FirstOrDefault(t => t.Name == "Seattle");
            var addresses = context.Addresses.Where(a => a.TownId == townToRemove.TownId);

            var count = addresses.Count();

            var employees = context.Employees.Where(e => addresses.Any(a => a.AddressId == e.AddressId));

            foreach (var employee in employees) employee.AddressId = null;
            foreach (var address in addresses) context.Addresses.Remove(address);

            context.Towns.Remove(townToRemove);

            context.SaveChanges();

            return $"{count} addresses in Seattle were deleted";
        }
    }
    
}