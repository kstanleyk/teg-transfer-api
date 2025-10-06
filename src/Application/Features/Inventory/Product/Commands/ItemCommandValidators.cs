using Agrovet.Application.Features.Inventory.Product.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.Product.Commands;

public abstract class ItemBaseValidator<T> : AbstractValidator<T>
    where T : BaseProductRequest
{
    protected void AddCommonRules(ItemValidationCodes validationCodes)
    {
        var validCategories = new HashSet<string>(validationCodes.CategoryCodes);

        RuleFor(i => i.Name)
            .NotEmpty().WithMessage("Product Name is required.")
            .NotNull().WithMessage("Product Name is required.")
            .MaximumLength(85).WithMessage("Product Name must not exceed 85 characters.");

        RuleFor(i => i.ShortDescription)
            .NotEmpty().WithMessage("Short Description is required.")
            .NotNull().WithMessage("Short Description is required.")
            .MaximumLength(250).WithMessage("Short Description must not exceed 250 characters.");

        RuleFor(i => i.BarCodeText)
            .NotEmpty().WithMessage("Bar Code is required.")
            .NotNull().WithMessage("Bar Code is required.")
            .MaximumLength(100).WithMessage("Bar Code must not exceed 100 characters.");

        RuleFor(i => i.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .NotNull().WithMessage("Brand is required.")
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

        RuleFor(i => i.Category)
            .NotEmpty().WithMessage("Category Code is required.")
            .NotNull().WithMessage("Category Code is required.")
            .MaximumLength(5).WithMessage("Category Code must not exceed 5 characters.")
            .Must(category => validCategories.Contains(category))
            .WithMessage("Invalid Category Code.");

        RuleFor(i => i.Status)
            .NotEmpty().WithMessage("Status is required.")
            .NotNull().WithMessage("Status is required.")
            .MaximumLength(5).WithMessage("Status must not exceed 5 characters.");

        RuleFor(i => i.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("MinStock cannot be negative.");

        RuleFor(i => i.MaxStock)
            .GreaterThanOrEqualTo(0).WithMessage("MaxStock cannot be negative.");

        RuleFor(i => i.ReorderLev)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder Level cannot be negative.");

        RuleFor(i => i.ReorderQtty)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder Quantity cannot be negative.");

        RuleFor(i => i.MaxStock)
            .GreaterThanOrEqualTo(i => i.MinStock).WithMessage("MaxStock cannot be less than MinStock.");

        RuleFor(i => i.ReorderLev)
            .LessThanOrEqualTo(i => i.MaxStock).WithMessage("Reorder Level cannot exceed MaxStock.");
    }
}

public class CreateItemValidator : ItemBaseValidator<CreateProductRequest>
{
    public CreateItemValidator(ItemValidationCodes validationCodes)
    {
        AddCommonRules(validationCodes);
    }
}

public class EditItemValidator : ItemBaseValidator<EditProductRequest>
{
    public EditItemValidator(ItemValidationCodes validationCodes)
    {
        RuleFor(i => i.Id)
            .NotEmpty().WithMessage("Product Code is required for edit.")
            .NotNull().WithMessage("Product Code is required for edit.")
            .MaximumLength(10).WithMessage("Product Code must not exceed 10 characters.");

        AddCommonRules(validationCodes);
    }
}

public class CreateItemCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateItemCommandValidator(ItemValidationCodes validationCodes)
    {
        RuleFor(p => p.Product)
            .NotNull().WithMessage("Product cannot be empty.")
            .SetValidator(new CreateItemValidator(validationCodes));
    }
}

public class EditItemCommandValidator : AbstractValidator<EditProductCommand>
{
    public EditItemCommandValidator(ItemValidationCodes validationCodes)
    {
        RuleFor(p => p.Product)
            .NotNull().WithMessage("Product cannot be empty.")
            .SetValidator(new EditItemValidator(validationCodes));
    }
}