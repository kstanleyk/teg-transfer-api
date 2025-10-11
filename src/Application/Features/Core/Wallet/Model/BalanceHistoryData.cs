using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallet.Model;

public class BalanceHistoryData
{
    public List<Ledger> Transactions { get; set; } = new();
    public decimal StartingBalance { get; set; }
    public List<DailyBalance> DailyBalances { get; set; } = new();
}