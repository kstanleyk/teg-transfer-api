using Agrovet.Application.Features.Inventory.OrderType.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderType.Commands;

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

        var orderType = Domain.Entity.Inventory.OrderType.Create(icr.Name);

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