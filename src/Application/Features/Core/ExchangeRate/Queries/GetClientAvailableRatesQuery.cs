using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetClientAvailableRatesQuery(
    Guid ClientId,
    Currency BaseCurrency,
    DateTime? AsOfDate = null) : IRequest<IReadOnlyList<ExchangeRateDto>>;