using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Warehouse.Commands;

public class CreateWarehouseCommandResponse : BaseResponse
{
    public WarehouseCreatedResponse Data { get; set; } = null!;
}

public class CreateWarehouseCommand : IRequest<CreateWarehouseCommandResponse>
{
    public required CreateWarehouseRequest Warehouse { get; set; }
}

public class CreateWarehouseCommandHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    :
        WarehouseCommandBase, IRequestHandler<CreateWarehouseCommand, CreateWarehouseCommandResponse>
{
    public async Task<CreateWarehouseCommandResponse> Handle(CreateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateWarehouseCommandResponse();

        var validator = new CreateWarehouseCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.Warehouse == null)
            throw new ArgumentNullException(nameof(request.Warehouse));

        var whr = request.Warehouse;

        var address = CreateAddress(request.Warehouse);

        var warehouse = Transfer.Domain.Entity.Inventory.Warehouse.Create(whr.Name, address);

        warehouse.SetPublicId(PublicId.CreateUnique().Value);

        var result = await warehouseRepository.AddAsync(warehouse);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<WarehouseCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        warehouseRepository.Dispose();
    }
}