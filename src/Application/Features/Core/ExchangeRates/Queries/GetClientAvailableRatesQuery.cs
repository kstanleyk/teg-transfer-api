using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetClientAvailableRatesQuery(
    Guid ClientId,
    Currency BaseCurrency,
    DateTime? AsOfDate = null) : IRequest<Result<IReadOnlyList<ExchangeRateDto>>>;

public class GetClientAvailableRatesQueryHandler(
    IExchangeRateRepository exchangeRateRepository,
    IClientRepository clientRepository,
    IMapper mapper)
    :RequestHandlerBase, IRequestHandler<GetClientAvailableRatesQuery, Result<IReadOnlyList<ExchangeRateDto>>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IReadOnlyList<ExchangeRateDto>>> Handle(GetClientAvailableRatesQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _clientRepository.GetAsync(query.ClientId);
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

    protected override void DisposeCore()
    {
        _exchangeRateRepository.Dispose();
        _clientRepository.Dispose();
    }
}