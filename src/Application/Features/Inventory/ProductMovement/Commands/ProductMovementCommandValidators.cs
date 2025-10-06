using Agrovet.Application.Features.Inventory.ProductMovement.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.ProductMovement.Commands;

public abstract class ProductMovementBaseValidator<T> : AbstractValidator<T>
    where T : BaseProductMovementRequest
{
    protected void AddCommonRules(ProductMovementValidationCodes validationCodes)
    {
        RuleFor(im => im.Id)
            .NotEmpty().WithMessage("PublicId is required.")
            .NotNull().WithMessage("PublicId is required.")
            .MaximumLength(50).WithMessage("PublicId must not exceed 50 characters.");

        RuleFor(im => im.LineNum)
            .NotEmpty().WithMessage("Line number is required.")
            .NotNull().WithMessage("Line number is required.")
            .MaximumLength(5).WithMessage("Line number must not exceed 5 characters.");

        RuleFor(im => im.Description)
            .NotEmpty().WithMessage("Description is required.")
            .NotNull().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(im => im.Item)
            .NotEmpty().WithMessage("Product is required.")
            .NotNull().WithMessage("Product is required.")
            .MaximumLength(100).WithMessage("Product must not exceed 100 characters.");

        RuleFor(im => im.TransDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .NotNull().WithMessage("Transaction date is required.");

        RuleFor(im => im.TransTime)
            .NotEmpty().WithMessage("Transaction time is required.")
            .NotNull().WithMessage("Transaction time is required.")
            .MaximumLength(10).WithMessage("Transaction time must not exceed 10 characters.");

        RuleFor(im => im.Sense)
            .NotEmpty().WithMessage("Sense is required.")
            .NotNull().WithMessage("Sense is required.")
            .MaximumLength(10).WithMessage("Sense must not exceed 10 characters.")
            .Must(s => s == "IN" || s == "OUT")
            .WithMessage("Sense must be either 'IN' or 'OUT'.");

        RuleFor(im => im.Qtty)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(im => im.SourceId)
            .NotEmpty().WithMessage("SourceId is required.")
            .NotNull().WithMessage("SourceId is required.")
            .MaximumLength(100).WithMessage("SourceId must not exceed 100 characters.");

        RuleFor(im => im.SourceLineNum)
            .NotEmpty().WithMessage("Source line number is required.")
            .NotNull().WithMessage("Source line number is required.")
            .MaximumLength(50).WithMessage("Source line number must not exceed 50 characters.");
    }
}

public class CreateProductMovementValidator : ProductMovementBaseValidator<CreateProductMovementRequest>
{
    public CreateProductMovementValidator(ProductMovementValidationCodes validationCodes)
    {
        AddCommonRules(validationCodes);
    }
}

public class EditProductMovementValidator : ProductMovementBaseValidator<EditProductMovementRequest>
{
    public EditProductMovementValidator(ProductMovementValidationCodes validationCodes)
    {
        RuleFor(im => im.Id)
            .NotEmpty().WithMessage("PublicId is required for edit.")
            .NotNull().WithMessage("PublicId is required for edit.")
            .MaximumLength(50).WithMessage("PublicId must not exceed 50 characters.");

        AddCommonRules(validationCodes);
    }
}

public class CreateProductMovementCommandValidator : AbstractValidator<CreateProductMovementCommand>
{
    public CreateProductMovementCommandValidator(ProductMovementValidationCodes validationCodes)
    {
        RuleFor(p => p.ProductMovement)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateProductMovementValidator(validationCodes));
    }
}

public class EditProductMovementCommandValidator : AbstractValidator<EditProductMovementCommand>
{
    public EditProductMovementCommandValidator(ProductMovementValidationCodes validationCodes)
    {
        RuleFor(p => p.ProductMovement)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditProductMovementValidator(validationCodes));
    }
}