using Agrovet.Application.Features.Inventory.OrderStatus.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.OrderStatus.Commands;

public abstract class OrderStatusBaseValidator<T> : AbstractValidator<T>
    where T : BaseOrderStatusRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateOrderStatusValidator : OrderStatusBaseValidator<CreateOrderStatusRequest>
{
    public CreateOrderStatusValidator()
    {
        AddCommonRules();
    }
}

public class EditOrderStatusValidator : OrderStatusBaseValidator<EditOrderStatusRequest>
{
    public EditOrderStatusValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateOrderStatusCommandValidator : AbstractValidator<CreateOrderStatusCommand>
{
    public CreateOrderStatusCommandValidator()
    {
        RuleFor(p => p.OrderStatus)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateOrderStatusValidator());
    }
}

public class EditOrderStatusCommandValidator : AbstractValidator<EditOrderStatusCommand>
{
    public EditOrderStatusCommandValidator()
    {
        RuleFor(p => p.OrderStatus)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditOrderStatusValidator());
    }
}