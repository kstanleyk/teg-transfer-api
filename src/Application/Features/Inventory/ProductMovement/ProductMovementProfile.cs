using Agrovet.Application.Features.Inventory.ProductMovement.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.ProductMovement;

public class ProductMovementProfile : Profile
{
    public ProductMovementProfile()
    {
        CreateMap<Domain.Entity.Inventory.ProductMovement, ProductMovementResponse>();
    }
}