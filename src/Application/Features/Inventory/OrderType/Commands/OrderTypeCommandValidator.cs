using FluentValidation;
using Transfer.Application.Features.Inventory.OrderType.Dtos;

namespace Transfer.Application.Features.Inventory.OrderType.Commands;

public abstract class OrderTypeBaseValidator<T> : AbstractValidator<T>
    where T : BaseOrderTypeRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateOrderTypeValidator : OrderTypeBaseValidator<CreateOrderTypeRequest>
{
    public CreateOrderTypeValidator()
    {
        AddCommonRules();
    }
}

public class EditOrderTypeValidator : OrderTypeBaseValidator<EditOrderTypeRequest>
{
    public EditOrderTypeValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateOrderTypeCommandValidator : AbstractValidator<CreateOrderTypeCommand>
{
    public CreateOrderTypeCommandValidator()
    {
        RuleFor(p => p.OrderType)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateOrderTypeValidator());
    }
}

public class EditOrderTypeCommandValidator : AbstractValidator<EditOrderTypeCommand>
{
    public EditOrderTypeCommandValidator()
    {
        RuleFor(p => p.OrderType)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditOrderTypeValidator());
    }
}