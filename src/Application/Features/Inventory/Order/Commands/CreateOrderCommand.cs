using Agrovet.Application.Features.Inventory.Order.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;
using SequentialGuid;

namespace Agrovet.Application.Features.Inventory.Order.Commands;

public class CreateOrderCommandResponse : BaseResponse
{
    public OrderCreatedResponse Data { get; set; } = null!;
}

public class CreateOrderCommand : IRequest<CreateOrderCommandResponse>
{
    public required CreateOrderRequest Order { get; set; }
}

public class CreateOrderCommandHandler(IOrderRepository averageWeightRepository,
    IEstateRepository estateRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateOrderCommand, CreateOrderCommandResponse>
{
    public async Task<CreateOrderCommandResponse> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateOrderCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new OrderValidationCodes
        {
            ValidSuppliers = ids
        };

        var validator = new CreateOrderCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.Order == null)
            throw new ArgumentNullException(nameof(request.Order));

        var or = request.Order;

        var order = Domain.Entity.Inventory.Order.Create(or.OrderType, or.OrderDate, or.Status, or.Description,
            or.Supplier, or.TransDate, DateTime.UtcNow);

        order.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await averageWeightRepository.AddAsync(order);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
        estateRepository.Dispose();
    }
}