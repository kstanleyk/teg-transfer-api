using AutoMapper;
using Transfer.Application.Features.Inventory.ProductMovement.Dtos;

namespace Transfer.Application.Features.Inventory.ProductMovement;

public class ProductMovementProfile : Profile
{
    public ProductMovementProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.ProductMovement, ProductMovementResponse>();
    }
}