using Agrovet.Application.Features.Inventory.Supplier.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.Supplier.Commands;

public abstract class SupplierBaseValidator<T> : AbstractValidator<T>
    where T : BaseSupplierRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .NotNull().WithMessage("Supplier name is required.")
            .MaximumLength(150).WithMessage("Supplier name must not exceed 150 characters.");

        RuleFor(c => c.Address)
            .NotEmpty().WithMessage("Supplier address is required.")
            .NotNull().WithMessage("Supplier address is required.")
            .MaximumLength(50).WithMessage("Supplier address must not exceed 50 characters.");

        RuleFor(c => c.City)
            .NotEmpty().WithMessage("Supplier city is required.")
            .NotNull().WithMessage("Supplier city is required.")
            .MaximumLength(50).WithMessage("Supplier city must not exceed 50 characters.");

        RuleFor(c => c.Phone)
            .NotEmpty().WithMessage("Supplier phone is required.")
            .NotNull().WithMessage("Supplier phone is required.")
            .MaximumLength(25).WithMessage("Supplier phone must not exceed 25 characters.");

        RuleFor(c => c.ContactPerson)
            .NotEmpty().WithMessage("Supplier contact person is required.")
            .NotNull().WithMessage("Supplier contact person is required.")
            .MaximumLength(100).WithMessage("Supplier contact person must not exceed 100 characters.");
    }
}


public class CreateSupplierValidator : SupplierBaseValidator<CreateSupplierRequest>
{
    public CreateSupplierValidator()
    {
        AddCommonRules();
    }
}

public class EditSupplierValidator : SupplierBaseValidator<EditSupplierRequest>
{
    public EditSupplierValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Supplier PublicId is required.")
            .NotNull().WithMessage("Supplier PublicId is required.")
            .MaximumLength(5).WithMessage("Supplier PublicId must not exceed 5 characters.");

        AddCommonRules();
    }
}

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(p => p.Supplier)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateSupplierValidator());
    }
}

public class EditSupplierCommandValidator : AbstractValidator<EditSupplierCommand>
{
    public EditSupplierCommandValidator()
    {
        RuleFor(p => p.Supplier)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditSupplierValidator());
    }
}