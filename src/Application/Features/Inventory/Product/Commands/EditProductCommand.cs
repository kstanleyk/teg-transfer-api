using Agrovet.Application.Features.Inventory.Product.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Product.Commands;

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
    IProductCategoryRepository productCategoryRepository,
    IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditProductCommand, EditProductCommandResponse>
{
    public async Task<EditProductCommandResponse> Handle(EditProductCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditProductCommandResponse();

        var categoryIds = await productCategoryRepository.GetAllIdsAsync();
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
        var bottlingType = Domain.Entity.Inventory.BottlingType.FiveLiters;

        var product = Domain.Entity.Inventory.Product.Create(brand, bottlingType, productRequest.Category, productRequest.Status,
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
        productCategoryRepository.Dispose();
    }
}