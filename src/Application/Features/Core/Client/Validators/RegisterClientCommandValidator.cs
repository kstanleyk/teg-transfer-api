using FluentValidation;
using TegWallet.Application.Features.Core.Client.Commands;

namespace TegWallet.Application.Features.Core.Client.Validators;

public class RegisterClientCommandValidator : AbstractValidator<RegisterClientCommand>
{
    public RegisterClientCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .Must(BeAUniqueEmail).WithMessage("Email is already registered")
            .WhenAsync(async (command, cancellationToken) =>
                await IsNewClientRegistration(command, cancellationToken));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+[1-9]\d{1,14}$").WithMessage("Phone number must be in E.164 format (e.g., +2348012345678)")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches("^[a-zA-Z]+$").WithMessage("First name can only contain letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .Matches("^[a-zA-Z]+$").WithMessage("Last name can only contain letters");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeAValidCurrency).WithMessage("Unsupported currency code. Supported: USD, NGN, XOF");

        // Cross-property validation
        RuleFor(x => x)
            .Must(HaveValidNameCombination).WithMessage("First name and last name cannot be the same")
            .Must(BeAtLeast18YearsOldIfRequired).WithMessage("Client must be at least 18 years old")
            .When(x => RequiresAgeVerification(x.PhoneNumber));
    }

    private static bool BeAValidCurrency(string? currencyCode)
    {
        var supportedCurrencies = new[] { "USD", "NGN", "XOF" };
        return supportedCurrencies.Contains(currencyCode?.ToUpper());
    }

    private static bool HaveValidNameCombination(RegisterClientCommand command)
    {
        return !string.Equals(command.FirstName, command.LastName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool BeAtLeast18YearsOldIfRequired(RegisterClientCommand command)
    {
        // Implement age verification logic if needed for your business
        // For now, return true as age verification might not be required
        return true;
    }

    private static bool RequiresAgeVerification(string phoneNumber)
    {
        // Implement country-specific age verification requirements
        // For example, some countries might require age verification
        return false;
    }

    private static bool BeAUniqueEmail(string email)
    {
        // This will be handled in the command handler with actual database check
        // This method is for client-side validation if needed
        return true;
    }

    private static async Task<bool> IsNewClientRegistration(RegisterClientCommand command, CancellationToken cancellationToken)
    {
        // This method can be used for async validation if needed
        return true;
    }
}