using Agrovet.Application.Features.Inventory.Supplier.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Supplier.Commands;

public class CreateSupplierCommandResponse : BaseResponse
{
    public SupplierCreatedResponse Data { get; set; } = null!;
}

public class CreateSupplierCommand : IRequest<CreateSupplierCommandResponse>
{
    public required CreateSupplierRequest Supplier { get; set; }
}

public class CreateSupplierCommandHandler(ISupplierRepository itemCategoryRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateSupplierCommand, CreateSupplierCommandResponse>
{
    public async Task<CreateSupplierCommandResponse> Handle(CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateSupplierCommandResponse();

        var validator = new CreateSupplierCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.Supplier == null)
            throw new ArgumentNullException(nameof(request.Supplier));

        var sr = request.Supplier;

        var supplier =
            Domain.Entity.Inventory.Supplier.Create(sr.Name, sr.Address, sr.City, sr.Phone, sr.ContactPerson);

        supplier.SetPublicId(PublicId.CreateUnique().Value);

        var result = await itemCategoryRepository.AddAsync(supplier);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<SupplierCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
    }
}