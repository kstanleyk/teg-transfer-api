using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.Currency.Dto
{
    public class CurrencyDto
    {
        public string Code { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public decimal DecimalPlaces { get; set; }
    }
}
