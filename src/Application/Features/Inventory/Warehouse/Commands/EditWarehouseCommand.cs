using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Warehouse.Commands;

public class EditWarehouseCommandResponse : BaseResponse
{
    public WarehouseUpdatedResponse Data { get; set; } = null!;
}

public class EditWarehouseCommand : IRequest<EditWarehouseCommandResponse>
{
    public required EditWarehouseRequest Warehouse { get; set; }
}

public class EditWarehouseCommandHandler(IWarehouseRepository warehouseRepository, IMapper mapper) 
    : WarehouseCommandBase, IRequestHandler<EditWarehouseCommand, EditWarehouseCommandResponse>
{
    public async Task<EditWarehouseCommandResponse> Handle(EditWarehouseCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditWarehouseCommandResponse();

        var validator = new EditWarehouseCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var whr = request.Warehouse;

        var address = CreateAddress(request.Warehouse);
        var warehouse = Transfer.Domain.Entity.Inventory.Warehouse.Create(whr.Name, address);

        warehouse.SetId(whr.Id);
        warehouse.SetPublicId(whr.PublicId);

        var result = await warehouseRepository.UpdateAsyncAsync(whr.PublicId, warehouse);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<WarehouseUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        warehouseRepository.Dispose();
    }
}