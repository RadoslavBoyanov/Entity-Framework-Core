using AutoMapper;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            //Import
            this.CreateMap<SuppliersDto, Supplier>();
            this.CreateMap<PartsDto, Part>();
            this.CreateMap<CustomerDto, Customer>();
            this.CreateMap<SaleDto, Sale>();
            //Export
            this.CreateMap<Car, ExportCarsDto>();
            this.CreateMap<Part, ExportPartDto>();
            this.CreateMap<Customer, CustomerDto>();
        }
    }
}
