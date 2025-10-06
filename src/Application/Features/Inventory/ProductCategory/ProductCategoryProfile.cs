using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.ProductCategory;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<Domain.Entity.Inventory.ProductCategory, ProductCategoryResponse>();

        CreateMap<Domain.Entity.Inventory.ProductCategory, ProductCategoryUpdatedResponse>();

        CreateMap<Domain.Entity.Inventory.ProductCategory, ProductCategoryCreatedResponse>();
    }
}