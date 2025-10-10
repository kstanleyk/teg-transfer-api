using AutoMapper;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;

namespace Transfer.Application.Features.Inventory.Warehouse;

public class WarehouseProfile : Profile
{
    public WarehouseProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.Warehouse, WarehouseResponse>()
            .ForMember(dest => dest.Street,
                opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.City,
                opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.State,
                opt => opt.MapFrom(src => src.Address.State))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => src.Address.Country))
            .ForMember(dest => dest.ZipCode,
                opt => opt.MapFrom(src => src.Address.ZipCode))
            .ForMember(dest => dest.Landmark,
                opt => opt.MapFrom(src => src.Address.Landmark));

        CreateMap<Transfer.Domain.Entity.Inventory.Warehouse, WarehouseUpdatedResponse>();
        CreateMap<Transfer.Domain.Entity.Inventory.Warehouse, WarehouseCreatedResponse>();
    }
}