using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public record ExtendExchangeRateValidityRequestDto(
    DateTime NewEffectiveTo,
    string UpdatedBy,
    string Reason = "Validity extended");