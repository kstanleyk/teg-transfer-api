using Agrovet.Application.Features.Inventory.OrderStatus.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderStatus.Commands;

public class CreateOrderStatusCommandResponse : BaseResponse
{
    public OrderStatusCreatedResponse Data { get; set; } = null!;
}

public class CreateOrderStatusCommand : IRequest<CreateOrderStatusCommandResponse>
{
    public required CreateOrderStatusRequest OrderStatus { get; set; }
}

public class CreateOrderStatusCommandHandler(IOrderStatusRepository orderStatusRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateOrderStatusCommand, CreateOrderStatusCommandResponse>
{
    public async Task<CreateOrderStatusCommandResponse> Handle(CreateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateOrderStatusCommandResponse();

        var validator = new CreateOrderStatusCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.OrderStatus == null)
            throw new ArgumentNullException(nameof(request.OrderStatus));

        var icr = request.OrderStatus;

        var orderStatus = Domain.Entity.Inventory.OrderStatus.Create(icr.Name);

        orderStatus.SetPublicId(PublicId.CreateUnique().Value);

        var result = await orderStatusRepository.AddAsync(orderStatus);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderStatusCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        orderStatusRepository.Dispose();
    }
}