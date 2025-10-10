using AutoMapper;
using Transfer.Application.Features.Inventory.Product.Dtos;

namespace Transfer.Application.Features.Inventory.Product;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.Product, ProductResponse>()
            .ForMember(dest => dest.Brand,
                opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.SizeInLitters,
                opt => opt.MapFrom(src => src.BottlingType.DisplayName));
        CreateMap<Transfer.Domain.Entity.Inventory.Product, ProductCreatedResponse>();
        CreateMap<Transfer.Domain.Entity.Inventory.Product, ProductUpdatedResponse>();
    }
}