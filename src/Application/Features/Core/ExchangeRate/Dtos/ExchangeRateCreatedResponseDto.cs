using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

// Response DTOs
public record ExchangeRateCreatedResponseDto
{
    public Guid ExchangeRateId { get; init; }
    public string? Message { get; init; }
}
