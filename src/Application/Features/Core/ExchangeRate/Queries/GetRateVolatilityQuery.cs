using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetRateVolatilityQuery(
    Currency BaseCurrency,
    Currency TargetCurrency,
    DateTime FromDate,
    DateTime ToDate,
    int PeriodDays = 30) : IRequest<RateVolatilityDto>;