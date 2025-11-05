using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetRateEffectivenessReportQuery(
    DateTime FromDate,
    DateTime ToDate) : IRequest<RateEffectivenessReportDto>;