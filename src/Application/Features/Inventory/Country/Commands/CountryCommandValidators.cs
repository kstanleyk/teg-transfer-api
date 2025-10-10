using FluentValidation;
using Transfer.Application.Features.Inventory.Country.Dtos;

namespace Transfer.Application.Features.Inventory.Country.Commands;

public abstract class CountryBaseValidator<T> : AbstractValidator<T>
    where T : BaseCountryRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateCountryValidator : CountryBaseValidator<CreateCountryRequest>
{
    public CreateCountryValidator()
    {
        AddCommonRules();
    }
}

public class EditCountryValidator : CountryBaseValidator<EditCountryRequest>
{
    public EditCountryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(p => p.Country)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateCountryValidator());
    }
}

public class EditCountryCommandValidator : AbstractValidator<EditCountryCommand>
{
    public EditCountryCommandValidator()
    {
        RuleFor(p => p.Country)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditCountryValidator());
    }
}