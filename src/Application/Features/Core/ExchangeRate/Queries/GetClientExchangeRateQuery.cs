using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetClientExchangeRateQuery(
    Guid ClientId,
    Currency BaseCurrency,
    Currency TargetCurrency,
    DateTime? AsOfDate = null) : IRequest<Result<ExchangeRateDto?>>;

public class GetClientExchangeRateQueryHandler(
    IExchangeRateRepository exchangeRateRepository,
    UserManager<Domain.Entity.Core.Client> userManager,
    IMapper mapper)
    : IRequestHandler<GetClientExchangeRateQuery, Result<ExchangeRateDto?>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly UserManager<Domain.Entity.Core.Client> _userManager = userManager;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ExchangeRateDto?>> Handle(GetClientExchangeRateQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _userManager.FindByIdAsync(query.ClientId.ToString());
            if (client == null)
                return Result<ExchangeRateDto?>.Failed("Client not found");

            var asOfDate = query.AsOfDate ?? DateTime.UtcNow;

            // Get the most specific rate available (Individual > Group > General)
            var rate = await _exchangeRateRepository.GetEffectiveRateForClientAsync(
                query.ClientId,
                client.ClientGroupId,
                query.BaseCurrency,
                query.TargetCurrency,
                asOfDate);

            if (rate == null)
                return Result<ExchangeRateDto?>.Succeeded(null);

            var rateDto = _mapper.Map<ExchangeRateDto>(rate);
            return Result<ExchangeRateDto?>.Succeeded(rateDto);
        }
        catch (Exception ex)
        {
            // Log exception here if needed
            return Result<ExchangeRateDto?>.Failed($"An error occurred while retrieving exchange rate: {ex.Message}");
        }
    }
}