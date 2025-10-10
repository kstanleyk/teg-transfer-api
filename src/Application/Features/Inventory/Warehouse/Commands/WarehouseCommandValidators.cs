using FluentValidation;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;

namespace Transfer.Application.Features.Inventory.Warehouse.Commands;

public abstract class WarehouseBaseValidator<T> : AbstractValidator<T>
    where T : BaseWarehouseRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateWarehouseValidator : WarehouseBaseValidator<CreateWarehouseRequest>
{
    public CreateWarehouseValidator()
    {
        AddCommonRules();
    }
}

public class EditWarehouseValidator : WarehouseBaseValidator<EditWarehouseRequest>
{
    public EditWarehouseValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(p => p.Warehouse)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateWarehouseValidator());
    }
}

public class EditWarehouseCommandValidator : AbstractValidator<EditWarehouseCommand>
{
    public EditWarehouseCommandValidator()
    {
        RuleFor(p => p.Warehouse)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditWarehouseValidator());
    }
}