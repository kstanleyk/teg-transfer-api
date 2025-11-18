using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Dtos;

public record MinimumAmountConfigurationCreatedResponseDto
{
    public Guid ConfigurationId { get; init; } = Guid.Empty;
}