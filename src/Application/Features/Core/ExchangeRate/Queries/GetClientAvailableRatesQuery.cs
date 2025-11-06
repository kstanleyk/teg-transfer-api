using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetClientAvailableRatesQuery(
    Guid ClientId,
    Currency BaseCurrency,
    DateTime? AsOfDate = null) : IRequest<Result<IReadOnlyList<ExchangeRateDto>>>;

public class GetClientAvailableRatesQueryHandler(
    IExchangeRateRepository exchangeRateRepository,
    UserManager<Domain.Entity.Core.Client> userManager,
    IMapper mapper)
    : IRequestHandler<GetClientAvailableRatesQuery, Result<IReadOnlyList<ExchangeRateDto>>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly UserManager<Domain.Entity.Core.Client> _userManager = userManager;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IReadOnlyList<ExchangeRateDto>>> Handle(GetClientAvailableRatesQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _userManager.FindByIdAsync(query.ClientId.ToString());
            if (client == null)
                return Result<IReadOnlyList<ExchangeRateDto>>.Failed("Client not found");

            var asOfDate = query.AsOfDate ?? DateTime.UtcNow;

            var rates = await _exchangeRateRepository.GetClientAvailableRatesAsync(
                query.ClientId,
                query.BaseCurrency,
                asOfDate);

            if (!rates.Any())
                return Result<IReadOnlyList<ExchangeRateDto>>.Succeeded(new List<ExchangeRateDto>());

            var rateDtos = _mapper.Map<IReadOnlyList<ExchangeRateDto>>(rates);
            return Result<IReadOnlyList<ExchangeRateDto>>.Succeeded(rateDtos);
        }
        catch (Exception ex)
        {
            // Log exception here if needed
            return Result<IReadOnlyList<ExchangeRateDto>>.Failed($"An error occurred while retrieving available rates: {ex.Message}");
        }
    }
}