using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.OrderType.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.OrderType.Commands;

public class CreateOrderTypeCommandResponse : BaseResponse
{
    public OrderTypeCreatedResponse Data { get; set; } = null!;
}

public class CreateOrderTypeCommand : IRequest<CreateOrderTypeCommandResponse>
{
    public required CreateOrderTypeRequest OrderType { get; set; }
}

public class CreateOrderTypeCommandHandler(IOrderTypeRepository orderTypeRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateOrderTypeCommand, CreateOrderTypeCommandResponse>
{
    public async Task<CreateOrderTypeCommandResponse> Handle(CreateOrderTypeCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateOrderTypeCommandResponse();

        var validator = new CreateOrderTypeCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.OrderType == null)
            throw new ArgumentNullException(nameof(request.OrderType));

        var icr = request.OrderType;

        var orderType = Transfer.Domain.Entity.Inventory.OrderType.Create(icr.Name);

        orderType.SetPublicId(PublicId.CreateUnique().Value);

        var result = await orderTypeRepository.AddAsync(orderType);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderTypeCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        orderTypeRepository.Dispose();
    }
}