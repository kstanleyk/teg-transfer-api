using Agrovet.Application.Features.Inventory.Product.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.Product;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Domain.Entity.Inventory.Product, ProductResponse>()
            .ForMember(dest => dest.Brand,
                opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.SizeInLitters,
                opt => opt.MapFrom(src => src.BottlingType.DisplayName));
        CreateMap<Domain.Entity.Inventory.Product, ProductCreatedResponse>();
        CreateMap<Domain.Entity.Inventory.Product, ProductUpdatedResponse>();
    }
}