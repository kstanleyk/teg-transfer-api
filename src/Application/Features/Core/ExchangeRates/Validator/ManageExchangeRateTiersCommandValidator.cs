using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRates.Command;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates.Validator;

public class ManageExchangeRateTiersCommandValidator : AbstractValidator<ManageExchangeRateTiersCommand>
{
    public ManageExchangeRateTiersCommandValidator()
    {
        RuleFor(x => x.ExchangeRateId)
            .NotEmpty()
            .WithMessage("Exchange rate ID is required");

        RuleFor(x => x.Tiers)
            .NotEmpty()
            .WithMessage("At least one tier is required")
            .Must(tiers => tiers.Count <= 20)
            .WithMessage("Cannot add more than 20 tiers at once");

        RuleForEach(x => x.Tiers)
            .SetValidator(new ExchangeRateTierRequestDtoValidator());

        // Validate that tier ranges don't overlap
        RuleFor(x => x.Tiers)
            .Must(HaveNonOverlappingTiers)
            .WithMessage("Tier ranges cannot overlap")
            .When(x => x.Tiers != null && x.Tiers.Count > 1);

        // Validate that tiers have no gaps
        RuleFor(x => x.Tiers)
            .Must(HaveNoGaps)
            .WithMessage("Tier ranges must be continuous without gaps")
            .When(x => x.Tiers != null && x.Tiers.Count > 1);

        // Validate that first tier starts from 1
        RuleFor(x => x.Tiers)
            .Must(FirstTierStartsFromOne)
            .WithMessage("The first tier must start from amount 1")
            .When(x => x.Tiers != null && x.Tiers.Any());
    }

    private bool HaveNonOverlappingTiers(List<ExchangeRateTierRequestDto>? tiers)
    {
        if (tiers == null || tiers.Count < 2) return true;

        var sortedTiers = tiers.OrderBy(t => t.MinAmount).ToList();

        for (int i = 0; i < sortedTiers.Count - 1; i++)
        {
            var currentTier = sortedTiers[i];
            var nextTier = sortedTiers[i + 1];

            if (currentTier.MaxAmount >= nextTier.MinAmount)
            {
                return false;
            }
        }

        return true;
    }

    private bool HaveNoGaps(List<ExchangeRateTierRequestDto>? tiers)
    {
        if (tiers == null || tiers.Count < 2) return true;

        var sortedTiers = tiers.OrderBy(t => t.MinAmount).ToList();

        for (int i = 0; i < sortedTiers.Count - 1; i++)
        {
            var currentTier = sortedTiers[i];
            var nextTier = sortedTiers[i + 1];

            // Check if there's a gap between current tier's max and next tier's min
            if (nextTier.MinAmount != currentTier.MaxAmount + 1)
            {
                return false;
            }
        }

        return true;
    }

    private bool FirstTierStartsFromOne(List<ExchangeRateTierRequestDto>? tiers)
    {
        if (tiers == null || !tiers.Any()) return true;

        var firstTier = tiers.OrderBy(t => t.MinAmount).First();
        return firstTier.MinAmount == 1;
    }
}
public class ExchangeRateTierRequestDtoValidator : AbstractValidator<ExchangeRateTierRequestDto>
{
    public ExchangeRateTierRequestDtoValidator()
    {
        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Min amount must be greater than or equal to 0")
            .LessThan(x => x.MaxAmount)
            .WithMessage("Min amount must be less than max amount");

        RuleFor(x => x.MaxAmount)
            .GreaterThan(x => x.MinAmount)
            .WithMessage("Max amount must be greater than min amount")
            .LessThanOrEqualTo(1000000) // 1 million
            .WithMessage("Max amount cannot exceed 1,000,000");

        //RuleFor(x => x.Rate)
        //    .GreaterThan(0)
        //    .WithMessage("Rate must be greater than 0")
        //    .LessThan(10000)
        //    .WithMessage("Rate cannot exceed 10,000");

        RuleFor(x => x.Margin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Margin must be greater than or equal to 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Margin cannot exceed 100%");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Created by is required")
            .MaximumLength(100)
            .WithMessage("Created by cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9@._-]+$")
            .WithMessage("Created by can only contain letters, numbers, @, ., _, and -");
    }
}