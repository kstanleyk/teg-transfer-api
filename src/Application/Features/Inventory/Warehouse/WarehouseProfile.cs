using AutoMapper;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;

namespace Transfer.Application.Features.Inventory.Warehouse;

public class WarehouseProfile : Profile
{
    public WarehouseProfile()
    {
        CreateMap<Domain.Entity.Core.Warehouse, WarehouseResponse>()
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

        CreateMap<Domain.Entity.Core.Warehouse, WarehouseUpdatedResponse>();
        CreateMap<Domain.Entity.Core.Warehouse, WarehouseCreatedResponse>();
    }
}