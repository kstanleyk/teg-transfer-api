using Agrovet.Application.Features.Inventory.Supplier.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Supplier.Commands;

public class EditSupplierCommandResponse : BaseResponse
{
    public SupplierUpdatedResponse Data { get; set; } = null!;
}

public class EditSupplierCommand : IRequest<EditSupplierCommandResponse>
{
    public required EditSupplierRequest Supplier { get; set; }
}

public class EditSupplierCommandHandler(ISupplierRepository itemCategoryRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditSupplierCommand, EditSupplierCommandResponse>
{
    public async Task<EditSupplierCommandResponse> Handle(EditSupplierCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditSupplierCommandResponse();

        var validator = new EditSupplierCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var sr = request.Supplier;

        var supplier =
            Domain.Entity.Inventory.Supplier.Create(sr.Name, sr.Address, sr.City, sr.Phone, sr.ContactPerson);

        supplier.SetId(sr.Id);
        supplier.SetPublicId(sr.PublicId);

        var result = await itemCategoryRepository.UpdateAsyncAsync(sr.PublicId, supplier);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<SupplierUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
    }
}