using AutoMapper;
using Transfer.Application.Features.Inventory.Category.Dtos;

namespace Transfer.Application.Features.Inventory.Category;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.Category, CategoryResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.Category, CategoryUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.Category, CategoryCreatedResponse>();
    }
}