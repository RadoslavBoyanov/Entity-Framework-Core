using System.ComponentModel.DataAnnotations;
using System.Text;
using Artillery.Data.Models;
using Artillery.Data.Models.Enums;
using Artillery.DataProcessor.ImportDto;
using Newtonsoft.Json;

namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage =
            "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Countries";
            ImportCountriesDto[] countriesDtos = Deserialize<ImportCountriesDto[]>(xmlString, rootName);

            List<Country> countries = new List<Country>();

            foreach (var cDto in countriesDtos)
            {
                if (!IsValid(cDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Country country = new Country()
                {
                    CountryName = cDto.CountryName,
                    ArmySize = cDto.ArmySize
                };

                countries.Add(country);
                output.AppendLine(string.Format(SuccessfulImportCountry, country.CountryName, country.ArmySize));
            }

            context.Countries.AddRange(countries);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Manufacturers";
            ImportManufacturesDto[] manufacturesDtos = Deserialize<ImportManufacturesDto[]>(xmlString, rootName);

            List<Manufacturer> manufactures = new List<Manufacturer>();

            foreach (var mDto in manufacturesDtos.Distinct())
            {
                var unique = manufactures.FirstOrDefault(x => x.ManufacturerName == mDto.ManufacturerName);

                if (!IsValid(mDto) || unique != null)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Manufacturer manufacturer = new Manufacturer()
                {
                    ManufacturerName = mDto.ManufacturerName,
                    Founded = string.Join(", ", mDto.Founded)
                };
                var foundedArray = mDto.Founded.Split(", ").ToArray();
                var countryName = foundedArray.Last();
                var townName = foundedArray[foundedArray.Length - 2];
                var townCountry = string.Join(", ", townName, countryName);

                manufactures.Add(manufacturer);
                output.AppendLine(string.Format(SuccessfulImportManufacturer, manufacturer.ManufacturerName, townCountry));
            }

            context.Manufacturers.AddRange(manufactures);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            StringBuilder output = new StringBuilder();

            string rootName = "Shells";
            ImportShellDto[] shellDtos = Deserialize<ImportShellDto[]>(xmlString, rootName);

            List<Shell> shells = new List<Shell>();

            foreach (var shellDto in shellDtos)
            {
                if (!IsValid(shellDto))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Shell shell = new Shell()
                {
                    ShellWeight = shellDto.ShellWeight,
                    Caliber = shellDto.Caliber
                };
                shells.Add(shell);
                output.AppendLine(string.Format(SuccessfulImportShell, shell.Caliber, shell.ShellWeight));
            }

            context.Shells.AddRange(shells);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            StringBuilder output = new StringBuilder();

            ImportGunsDto[] gunsDtos = JsonConvert.DeserializeObject<ImportGunsDto[]>(jsonString);

            List<Gun> guns = new List<Gun>();

            var gunType = new string[] 
            { 
                "Howitzer",
                "Mortar" ,
                "FieldGun" ,
                "AntiAircraftGun",
                "MountainGun",
                "AntiTankGun"
            };

            foreach (var gDto in gunsDtos)
            {
                if (!IsValid(gDto))
                {
                    output.AppendLine(ErrorMessage); 
                    continue;
                }

                if (!gunType.Contains(gDto.GunType))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                Gun gun = new Gun()
                {
                    ManufacturerId = gDto.ManufacturerId,
                    GunWeight = gDto.GunWeight,
                    BarrelLength = gDto.BarrelLength,
                    NumberBuild = gDto.NumberBuild,
                    Range = gDto.Range,
                    GunType = (GunType)Enum.Parse(typeof(GunType), gDto.GunType ),
                    ShellId = gDto.ShellId,
                };

                foreach (var cDto in gDto.Countries)
                {
                    CountryGun countryGun = new CountryGun()
                    {
                        CountryId = cDto.Id,
                        Gun = gun
                    };
                    gun.CountriesGuns.Add(countryGun);
                }

                guns.Add(gun);
                output.AppendLine(string.Format(SuccessfulImportGun, gun.GunType, gun.GunWeight, gun.BarrelLength));
            }

            context.Guns.AddRange(guns);
            context.SaveChanges();

            return output.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
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