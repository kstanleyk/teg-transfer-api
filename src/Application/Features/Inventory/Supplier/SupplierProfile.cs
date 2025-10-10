using AutoMapper;
using Transfer.Application.Features.Inventory.Supplier.Dtos;

namespace Transfer.Application.Features.Inventory.Supplier;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.Supplier, SupplierResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.Supplier, SupplierUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.Supplier, SupplierCreatedResponse>();
    }
}