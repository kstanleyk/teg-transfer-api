using Agrovet.Application.Features.Inventory.Item.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.Item.Commands;

public abstract class ItemBaseValidator<T> : AbstractValidator<T>
    where T : BaseItemRequest
{
    protected void AddCommonRules(ItemValidationCodes validationCodes)
    {
        var validCategories = new HashSet<string>(validationCodes.CategoryCodes);

        RuleFor(i => i.Name)
            .NotEmpty().WithMessage("Item Name is required.")
            .NotNull().WithMessage("Item Name is required.")
            .MaximumLength(85).WithMessage("Item Name must not exceed 85 characters.");

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

public class CreateItemValidator : ItemBaseValidator<CreateItemRequest>
{
    public CreateItemValidator(ItemValidationCodes validationCodes)
    {
        AddCommonRules(validationCodes);
    }
}

public class EditItemValidator : ItemBaseValidator<EditItemRequest>
{
    public EditItemValidator(ItemValidationCodes validationCodes)
    {
        RuleFor(i => i.Id)
            .NotEmpty().WithMessage("Item Code is required for edit.")
            .NotNull().WithMessage("Item Code is required for edit.")
            .MaximumLength(10).WithMessage("Item Code must not exceed 10 characters.");

        AddCommonRules(validationCodes);
    }
}

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator(ItemValidationCodes validationCodes)
    {
        RuleFor(p => p.Item)
            .NotNull().WithMessage("Item cannot be empty.")
            .SetValidator(new CreateItemValidator(validationCodes));
    }
}

public class EditItemCommandValidator : AbstractValidator<EditItemCommand>
{
    public EditItemCommandValidator(ItemValidationCodes validationCodes)
    {
        RuleFor(p => p.Item)
            .NotNull().WithMessage("Item cannot be empty.")
            .SetValidator(new EditItemValidator(validationCodes));
    }
}