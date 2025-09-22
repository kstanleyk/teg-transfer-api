using Agrovet.Application.Features.Core.AverageWeight.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Core.AverageWeight.Commands;

public abstract class AverageWeightBaseValidator<T>(AverageWeightValidationCodes validationCodes) : AbstractValidator<T>
    where T : BaseAverageWeightRequest
{
    private readonly HashSet<string> _validEstateCodes = new(validationCodes.EstateCodes, StringComparer.OrdinalIgnoreCase);

    protected void AddCommonRules()
    {
        RuleFor(p => p.Estate)
            .NotEmpty()
            .WithMessage("Estate Code is required.")
            .NotNull()
            .WithMessage("Estate Code is required.")
            .MaximumLength(5)
            .WithMessage("Estate Code must not exceed 5 characters.")
            .Must(code => _validEstateCodes.Contains(code))
            .WithMessage("Estate Code is not valid.");

        RuleFor(p => p.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .NotNull()
            .WithMessage("Status is required.")
            .MaximumLength(2)
            .WithMessage("Status must not exceed 2 characters.");
    }
}

public class CreateAverageWeightValidator : AverageWeightBaseValidator<CreateAverageWeightRequest>
{
    public CreateAverageWeightValidator(AverageWeightValidationCodes validationCodes)
        : base(validationCodes)
    {
        AddCommonRules();
    }
}

public class EditAverageWeightValidator : AverageWeightBaseValidator<EditAverageWeightRequest>
{
    public EditAverageWeightValidator(AverageWeightValidationCodes validationCodes)
        : base(validationCodes)
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("Code is required.")
            .NotNull().WithMessage("Code is required.")
            .MaximumLength(5).WithMessage("Code must not exceed 5 characters.");

        AddCommonRules();
    }
}

public class CreateAverageWeightCommandValidator : AbstractValidator<CreateAverageWeightCommand>
{
    public CreateAverageWeightCommandValidator(AverageWeightValidationCodes validationCodes)
    {
        RuleFor(p => p.AverageWeight)
            .NotNull().WithMessage("Average weight cannot be empty.")
            .SetValidator(new CreateAverageWeightValidator(validationCodes));
    }
}

public class EditAverageWeightCommandValidator : AbstractValidator<EditAverageWeightCommand>
{
    public EditAverageWeightCommandValidator(AverageWeightValidationCodes validationCodes)
    {
        RuleFor(p => p.AverageWeight)
            .NotNull().WithMessage("Average weight cannot be empty.")
            .SetValidator(new EditAverageWeightValidator(validationCodes));
    }
}
