using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class LedgerMapper
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    public decimal AmountAmount { get; init; }
    public string AmountCurrency { get; init; } = null!;
    public TransactionStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompletionType { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public Guid? ReservationId { get; set; }

    public Ledger ToLedger()
    {
        try
        {
            var money = new Money(AmountAmount, Currency.FromCode(AmountCurrency));
            var ledger = Ledger.Hydrate(WalletId, Type, money, Status, FailureReason, CompletionType, CompletedBy, CompletedAt, Reference, Description,
                Timestamp, ReservationId);
            ledger.SetId(Id);
            ledger.HydrateFields(FailureReason, CompletionType, CompletedBy, CompletedAt);
            return ledger;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}