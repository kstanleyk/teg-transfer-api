using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

public class RequestWithdrawFundsCommandValidator : AbstractValidator<RequestWithdrawFundsCommand>
{
    public RequestWithdrawFundsCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required")
            .NotEqual(Guid.Empty).WithMessage("Client ID must be a valid GUID");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Withdrawal amount must be greater than 0")
            .LessThan(1_000_000).WithMessage("Withdrawal amount cannot exceed 1,000,000");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeAValidCurrency).WithMessage("Unsupported currency code. Supported: USD, NGN, XOF");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Business rule: Minimum withdrawal amount based on currency
        RuleFor(x => x)
            .Must(HaveMinimumAmountForCurrency).WithMessage("Minimum withdrawal amount for {CurrencyCode} is {MinAmount}")
            .WithName("Amount");

        // Business rule: Check sufficient balance (this will be handled in domain, but we can do a basic check)
        RuleFor(x => x)
            .MustAsync(HasSufficientBalance).WithMessage("Insufficient balance for withdrawal")
            .When(x => x.Amount > 0);
    }

    private static bool BeAValidCurrency(string currencyCode)
    {
        var supportedCurrencies = new[] { "USD", "NGN", "XOF" };
        return supportedCurrencies.Contains(currencyCode?.ToUpper());
    }

    private bool HaveMinimumAmountForCurrency(RequestWithdrawFundsCommand command)
    {
        var minAmounts = new Dictionary<string, decimal>
        {
            ["USD"] = 5.00m,
            ["NGN"] = 500.00m,
            ["XOF"] = 1000.00m
        };

        if (minAmounts.TryGetValue(command.CurrencyCode.ToUpper(), out var minAmount))
        {
            return command.Amount >= minAmount;
        }

        return true;
    }

    private async Task<bool> HasSufficientBalance(RequestWithdrawFundsCommand command, CancellationToken cancellationToken)
    {
        // This is a basic check. The domain will do the actual balance validation.
        // In a real implementation, you might want to check the available balance here
        // For now, return true and let the domain handle the actual validation
        return true;
    }
}