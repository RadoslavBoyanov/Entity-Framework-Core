using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            //string inputJson = File.ReadAllText("../../../Datasets/sales.json");
            CarDealerContext dbContext = new CarDealerContext();
            //dbContext.Database.EnsureDeleted();
            //dbContext.Database.EnsureCreated();

            //Console.WriteLine("Database created successfully!");

            //Import
            //Console.WriteLine(ImportSuppliers(dbContext, inputJson));
            //Console.WriteLine(ImportParts(dbContext, inputJson));
            //Console.WriteLine(ImportCars(dbContext, inputJson));
            //Console.WriteLine(ImportCustomers(dbContext, inputJson));
            //Console.WriteLine(ImportSales(dbContext, inputJson));

            //Export
            //string json = GetOrderedCustomers(dbContext);
            //File.WriteAllText("../../../Results/ordered-customers.json", json);


            //string json = GetCarsFromMakeToyota(dbContext);
            //File.WriteAllText("../../../Results/toyota-cars.json", json);

            //string json = GetLocalSuppliers(dbContext);
            //File.WriteAllText("../../../Results/local-suppliers.json", json);

            //string json = GetCarsWithTheirListOfParts(dbContext);
            //File.WriteAllText("../../../Results/cars-and-parts.json", json);


            //string json = GetTotalSalesByCustomer(dbContext);
            //File.WriteAllText("../../../Results/customers-total-sales.json", json);

            string json = GetSalesWithAppliedDiscount(dbContext);
            File.WriteAllText("../../../Results/sales-discounts.json", json);
        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<CarDealerProfile>(); });

            var mapper = config.CreateMapper();

            SuppliersDto[] suppliersDto = JsonConvert.DeserializeObject<SuppliersDto[]>(inputJson);

            ICollection<Supplier> suppliers = new List<Supplier>();

            foreach (var suppDto in suppliersDto)
            {
                var supplier = mapper.Map<Supplier>(suppDto);
                suppliers.Add(supplier);
            }

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<CarDealerProfile>(); });

            var mapper = config.CreateMapper();

            PartsDto[] partsDto = JsonConvert.DeserializeObject<PartsDto[]>(inputJson);

            ICollection<Part> parts = new List<Part>();

            foreach (var partDto in partsDto)
            {
                if (partDto.SupplierId > 31)
                {
                    continue;
                }
                var part = mapper.Map<Part>(partDto);
                parts.Add(part);
            }

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            CarDto[] carsDto = JsonConvert.DeserializeObject<CarDto[]>(inputJson);

            ICollection<Car> cars = new List<Car>();

            foreach (var carDto in carsDto)
            {
                Car currentCar = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TraveledDistance = carDto.TravelledDistance
                };
                foreach (var carPartId in carDto.PartsId.Distinct())
                {
                    List<int> validId = context.Parts.Select(p => p.Id).ToList();
                    if (validId.Contains(carPartId))
                    {
                        currentCar.PartsCars.Add(new PartCar() { PartId = carPartId });
                    }
                }
                cars.Add(currentCar);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<CarDealerProfile>(); });

            var mapper = config.CreateMapper();

            CustomerDto[] customersDto = JsonConvert.DeserializeObject<CustomerDto[]>(inputJson);

            ICollection<Customer> customers = new List<Customer>();

            foreach (var customerDto in customersDto)
            {
                var currentCustomer = mapper.Map<Customer>(customerDto);
                customers.Add(currentCustomer);
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<CarDealerProfile>(); });

            var mapper = config.CreateMapper();

            SaleDto[] salesDto = JsonConvert.DeserializeObject<SaleDto[]>(inputJson);

            ICollection<Sale> sales = new List<Sale>();
            List<int> customerId = context.Customers.Select(c => c.Id).ToList();
            List<int> carId = context.Customers.Select(c => c.Id).ToList();

            foreach (var saleDto in salesDto)
            {
                var currentSale = mapper.Map<Sale>(saleDto);
                sales.Add(currentSale);
                
            }

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}.";
        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatString = "dd/MM/yyyy",
            };
            var customers = context.Customers
                .Select(c => new
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate,
                    IsYoungDriver = c.IsYoungDriver
                })
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver).ToList();

            var json = JsonConvert.SerializeObject(customers,Formatting.Indented ,settings);
            return json;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var carsFromMakeToyota = context.Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .Select(c => new
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(carsFromMakeToyota, Formatting.Indented);

            return json;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers.Where(s => s.IsImporter == false)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                }).ToList();

            var json = JsonConvert.SerializeObject(suppliers, Formatting.Indented);
            return json;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(c => new
                {
                    car = new
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TraveledDistance = c.TraveledDistance
                    },
                    parts = c.PartsCars.Select(pc => new
                    {
                        Name = pc.Part.Name,
                        Price = $"{pc.Part.Price:F2}"
                    }).ToArray(),
                }).ToArray();

            var jsonFile = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return jsonFile;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customerSales = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.Count(),
                    salePrices = c.Sales.SelectMany(x => x.Car.PartsCars.Select(x => x.Part.Price))
                })
                .ToArray();

            var totalSalesByCustomer = customerSales.Select(t => new
                {
                    t.fullName,
                    t.boughtCars,
                    spentMoney = t.salePrices.Sum()
                })
                .OrderByDescending(t => t.spentMoney)
                .ThenByDescending(t => t.boughtCars)
                .ToArray();

            return JsonConvert.SerializeObject(totalSalesByCustomer, Formatting.Indented);
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var salesWithDiscount = context.Sales
                .Take(10)
                .Select(s => new
                {
                    car = new
                    {
                        s.Car.Make,
                        s.Car.Model,
                        s.Car.TraveledDistance
                    },
                    customerName = s.Customer.Name,
                    discount = $"{s.Discount:f2}",
                    price = $"{s.Car.PartsCars.Sum(p => p.Part.Price):f2}",
                    priceWithDiscount = $"{s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - s.Discount / 100):f2}"
                })
                .ToArray();

            return JsonConvert.SerializeObject(salesWithDiscount, Formatting.Indented);
        }
    }
}