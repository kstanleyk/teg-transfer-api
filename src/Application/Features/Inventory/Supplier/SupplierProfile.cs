using Agrovet.Application.Features.Inventory.Supplier.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.Supplier;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<Domain.Entity.Inventory.Supplier, SupplierResponse>();

        CreateMap<Domain.Entity.Inventory.Supplier, SupplierUpdatedResponse>();

        CreateMap<Domain.Entity.Inventory.Supplier, SupplierCreatedResponse>();
    }
}