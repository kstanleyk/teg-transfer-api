using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Product.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Features.Inventory.Product.Commands;

public class EditProductCommandResponse : BaseResponse
{
    public ProductUpdatedResponse Data { get; set; } = null!;
}

public class EditProductCommand : IRequest<EditProductCommandResponse>
{
    public required EditProductRequest Product { get; set; }
}

public class EditItemCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditProductCommand, EditProductCommandResponse>
{
    public async Task<EditProductCommandResponse> Handle(EditProductCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditProductCommandResponse();

        var categoryIds = await categoryRepository.GetAllIdsAsync();
        var validationCodes = new ItemValidationCodes
        {
            CategoryCodes = categoryIds
        };

        var validator = new EditItemCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var productRequest = request.Product;

        var brand = Brand.FromName(productRequest.Brand);
        var bottlingType = Transfer.Domain.Entity.Inventory.BottlingType.FiveLiters;

        var product = Transfer.Domain.Entity.Inventory.Product.Create(brand, bottlingType, productRequest.Category, productRequest.Status,
            productRequest.MinStock, productRequest.MaxStock, productRequest.ReorderLev,
            productRequest.ReorderQtty, SkuGenerators.Deterministic, DateTime.UtcNow);

        product.SetId(productRequest.Id);
        product.SetPublicId(productRequest.PublicId);

        var result = await productRepository.UpdateAsyncAsync(product.PublicId, product);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ProductUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        productRepository.Dispose();
        categoryRepository.Dispose();
    }
}