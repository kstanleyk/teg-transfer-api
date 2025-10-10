using Transfer.Application.Features.Inventory.Warehouse.Dtos;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Warehouse.Commands;

public abstract class WarehouseCommandBase : RequestHandlerBase
{
    protected static Address CreateAddress(BaseWarehouseRequest request)
    {
        if (request.Country == "CM")
            return Address.CreateCameroonAddress(city: request.City, quarter: request.Quarter, landmark: request.Landmark,
                region: request.Region);
        else
            return Address.CreateUsAddress(street: request.Street, city: request.City, state: request.State, zipCode: request.ZipCode);
    }
}