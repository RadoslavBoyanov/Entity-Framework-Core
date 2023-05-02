using CarDealer.Data;
using System.Text;
using System.Xml.Serialization;
using CarDealer.DTOs.Export;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            var context = new CarDealerContext();

            //14
            //string xmlOutput = GetCarsWithDistance(context);
            //File.WriteAllText(@"../../../Results/cars.xml", xmlOutput);

            //15
            //string xmlOutput = GetCarsFromMakeBmw(context);
            //File.WriteAllText(@"../../../Results/bmw-cars.xml", xmlOutput);

            //17
            //string xmlOutput = GetCarsWithTheirListOfParts(context);
            //File.WriteAllText(@"../../../Results/cars-and-parts.xml", xmlOutput);

            //18
            //string xmlOutput = GetTotalSalesByCustomer(context);
            //File.WriteAllText(@"../../../Results/customers-total-sales.xml", xmlOutput);

            //19
            string xmlOutput = GetSalesWithAppliedDiscount(context);
            File.WriteAllText(@"../../../Results/sales-discounts.xml", xmlOutput);
        }

        // this is a generic XML Serializer
        private static string Serializer<T>(T dataTransferObjects, string xmlRootAttributeName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootAttributeName));

            StringBuilder sb = new StringBuilder();
            using var write = new StringWriter(sb);

            XmlSerializerNamespaces xmlNamespaces = new XmlSerializerNamespaces();
            xmlNamespaces.Add(string.Empty, string.Empty);

            serializer.Serialize(write, dataTransferObjects, xmlNamespaces);

            return sb.ToString();
        }

        //14
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            ExportTraveledCarDto[] carsDtos = context.Cars
                .Where(c => c.TraveledDistance > 2_000_000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new ExportTraveledCarDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TraveledDistance,
                })
                .ToArray();

            return Serializer<ExportTraveledCarDto[]>(carsDtos, "cars");
        }

        //15
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            CarBwmDto[] carsDtos = context.Cars
                .Where(c => c.Make == "BMW")
                .Select(c => new CarBwmDto()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                })
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .ToArray();

            return Serializer<CarBwmDto[]>(carsDtos, "cars");
        }

        //17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            ExportCarsWithPartsDto[] carsPartsDtos = context.Cars
                .OrderByDescending(c => c.TraveledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .Select(c => new ExportCarsWithPartsDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                    CarParts = c.PartsCars.Select(pc => new CartPartsDto()
                        {
                            Name = pc.Part.Name,
                            Price = pc.Part.Price
                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                })
                .ToArray();

            return Serializer<ExportCarsWithPartsDto[]>(carsPartsDtos, "cars");
        }

        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var tempDto = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    FullName = c.Name,
                    BoughtCars = c.Sales.Count(),
                    SalesInfo = c.Sales.Select(s => new
                    {
                        Prices = c.IsYoungDriver
                            ? s.Car.PartsCars.Sum(p => Math.Round((double)p.Part.Price * 0.95, 2))
                            : s.Car.PartsCars.Sum(p => (double)p.Part.Price)
                    }).ToArray(),
                })
                .ToArray();

            SalesByCustomerDto[] totalSalesDtos = tempDto
                .OrderByDescending(t => t.SalesInfo.Sum(s => s.Prices))
                .Select(t => new SalesByCustomerDto()
                {
                    FullName = t.FullName,
                    BoughtCars = t.BoughtCars,
                    SpentMoney = t.SalesInfo.Sum(s => s.Prices).ToString("f2")
                })
                .ToArray();

            return Serializer<SalesByCustomerDto[]>(totalSalesDtos, "customers");
        }

        //19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            SalesWithAppliedDiscountDto[] salesDtos = context
                .Sales
                .Select(s => new SalesWithAppliedDiscountDto()
                {
                    SingleCar = new SingleCar()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },
                    Discount = (int)s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartsCars.Sum(p => p.Part.Price),
                    PriceWithDiscount = Math.Round((double)(s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - (s.Discount / 100))), 4)
                })
                .ToArray();

            return Serializer<SalesWithAppliedDiscountDto[]>(salesDtos, "sales");
        }
    }
}