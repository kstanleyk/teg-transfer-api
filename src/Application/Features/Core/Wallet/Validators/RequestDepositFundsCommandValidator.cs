using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

public class RequestDepositFundsCommandValidator : AbstractValidator<RequestDepositFundsCommand>
{
    public RequestDepositFundsCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required")
            .NotEqual(Guid.Empty).WithMessage("Client ID must be a valid GUID");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("RequestDeposit amount must be greater than 0")
            .LessThan(1_000_000).WithMessage("RequestDeposit amount cannot exceed 1,000,000");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeAValidCurrency).WithMessage("Unsupported currency code. Supported: USD, NGN, XOF");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Business rule: Minimum deposit amount based on currency
        RuleFor(x => x)
            .Must(HaveMinimumAmountForCurrency).WithMessage("Minimum deposit amount for {CurrencyCode} is {MinAmount}")
            .WithName("Amount");

        // Business rule: Maximum deposit amount per day (hypothetical)
        RuleFor(x => x)
            .MustAsync(NotExceedDailyDepositLimit).WithMessage("Daily deposit limit exceeded");
    }

    private static bool BeAValidCurrency(string? currencyCode)
    {
        var supportedCurrencies = new[] { "USD", "NGN", "XOF" };
        return supportedCurrencies.Contains(currencyCode?.ToUpper());
    }

    private bool HaveMinimumAmountForCurrency(RequestDepositFundsCommand command)
    {
        var minAmounts = new Dictionary<string, decimal>
        {
            ["USD"] = 10.00m,
            ["NGN"] = 1000.00m,
            ["XOF"] = 5000.00m
        };

        if (minAmounts.TryGetValue(command.CurrencyCode.ToUpper(), out var minAmount))
        {
            return command.Amount >= minAmount;
        }

        return true;
    }

    private async Task<bool> NotExceedDailyDepositLimit(RequestDepositFundsCommand command, CancellationToken cancellationToken)
    {
        // This would typically check against a database or cache
        // For now, we'll implement a simple check
        var dailyLimit = GetDailyDepositLimit(command.CurrencyCode);

        // In a real implementation, you would query the database for today's deposits
        // var todayDeposits = await _transactionRepository.GetTodaysDepositsAsync(command.ClientId, DateTime.UtcNow.Date);
        // var totalToday = todayDeposits.Sum(t => t.Amount.Amount);
        // return (totalToday + command.Amount) <= dailyLimit;

        // For now, return true (assuming limit not exceeded)
        // In production, you would inject a repository and implement the actual check
        return true;
    }

    private decimal GetDailyDepositLimit(string currencyCode)
    {
        return currencyCode.ToUpper() switch
        {
            "USD" => 5000.00m,
            "NGN" => 500000.00m,
            "XOF" => 1000000.00m,
            _ => 10000.00m
        };
    }
}