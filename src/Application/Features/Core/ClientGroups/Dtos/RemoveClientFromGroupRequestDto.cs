using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.ClientGroups.Dtos;

public record RemoveClientFromGroupRequestDto(
    string Reason = "Removed from group");
