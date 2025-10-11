using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class CheckBalanceRequest
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = null!;
}