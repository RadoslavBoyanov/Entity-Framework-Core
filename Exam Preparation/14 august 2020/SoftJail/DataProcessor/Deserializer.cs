using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SoftJail.Data.Models;
using SoftJail.Data.Models.Enums;
using SoftJail.DataProcessor.ImportDto;

namespace SoftJail.DataProcessor
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Data;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";

        private const string SuccessfullyImportedDepartment = "Imported {0} with {1} cells";

        private const string SuccessfullyImportedPrisoner = "Imported {0} {1} years old";

        private const string SuccessfullyImportedOfficer = "Imported {0} ({1} prisoners)";

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            DepartmentImportDto[] departmentDtos = JsonConvert.DeserializeObject<DepartmentImportDto[]>(jsonString);
            
            List<Department> departments = new List<Department>();

            foreach (var dDto in departmentDtos)
            {
                if (!IsValid(dDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Department department = new Department()
                {
                    Name = dDto.Name,
                };

                bool isValidDep = true;
                foreach (var cellDto in dDto.Cells)
                {
                    if (!IsValid(cellDto))
                    {
                        isValidDep = false;
                        break;
                    }

                    Cell cell = new Cell()
                    {
                        CellNumber = cellDto.CellNumber,
                        HasWindow = cellDto.HasWindow,
                    };
                    department.Cells.Add(cell);
                }

                if (!isValidDep)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                if (department.Cells.Count == 0)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                departments.Add(department);
                output.AppendLine(
                    String.Format(SuccessfullyImportedDepartment, department.Name, department.Cells.Count));
            }

            context.AddRange(departments);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            PrisonersImportDto[] prisonersDtos = JsonConvert.DeserializeObject<PrisonersImportDto[]>(jsonString);

            List<Prisoner> prisoners = new List<Prisoner>();

            foreach (var pDto in prisonersDtos)
            {
                if (!IsValid(pDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime dateIncarceration;
                bool isValidIncarcerationDate = DateTime.TryParseExact(pDto.IncarcerationDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out dateIncarceration);

                if (!isValidIncarcerationDate)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime? releaseDate = null;

                if (!String.IsNullOrEmpty(pDto.ReleaseDate))
                {
                    DateTime releaseDateWithValue;
                    bool isValidReleaseDate = DateTime.TryParseExact(pDto.ReleaseDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDateWithValue);

                    if (!isValidReleaseDate)
                    {
                        output.Append(ErrorMessage);
                        continue;
                    }

                    releaseDate = releaseDateWithValue;
                }

                Prisoner prisoner = new Prisoner()
                {
                    FullName = pDto.FullName,
                    Nickname = pDto.Nickname,
                    Age = pDto.Age,
                    IncarcerationDate = dateIncarceration,
                    ReleaseDate = releaseDate,
                    Bail = pDto.Bail,
                    CellId = pDto.CellId
                };

                bool IsValidAllMails = true;
                foreach (var mailDto in pDto.Mails)
                {
                    if (!IsValid(mailDto))
                    {
                        IsValidAllMails = false;
                        continue;
                    }

                    prisoner.Mails.Add(new Mail()
                    {
                        Description = mailDto.Description,
                        Sender = mailDto.Sender,
                        Address = mailDto.Address
                    });
                }

                if (!IsValidAllMails)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                prisoners.Add(prisoner);
                output.AppendLine(String.Format(SuccessfullyImportedPrisoner, prisoner.FullName, prisoner.Age));
            }

            context.AddRange(prisoners);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Officers";
            OfficersImportDto[] officersDtos = Deserialize<OfficersImportDto[]>(xmlString, rootName);

            List<Officer> officers = new List<Officer>();

            foreach (var oDto in officersDtos)
            {
                if (!IsValid(oDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                object positionObj;
                bool isValidPositions = Enum.TryParse(typeof(Position), oDto.Position, out positionObj);

                if (!isValidPositions)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                object weaponObj;
                bool isValidWeapons = Enum.TryParse(typeof(Weapon), oDto.Weapon, out weaponObj);

                if (!isValidWeapons)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Officer officer = new Officer()
                {
                    FullName = oDto.FullName,
                    Salary = oDto.Salary,
                    Position = (Position)positionObj,
                    Weapon = (Weapon)weaponObj,
                    DepartmentId = oDto.DepartmentId
                };

                foreach (var prisonerId in oDto.Prisoners)
                {
                    OfficerPrisoner officerPrisoner = new OfficerPrisoner()
                    {
                        PrisonerId = prisonerId.Id
                    };

                    officer.OfficerPrisoners.Add(officerPrisoner);
                }
                officers.Add(officer);
                output.AppendLine(String.Format(SuccessfullyImportedOfficer, officer.FullName,
                    officer.OfficerPrisoners.Count));
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
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