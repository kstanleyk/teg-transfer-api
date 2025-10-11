namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string FriendlyId { get; set; } = string.Empty;
    public Guid WalletId { get; set; }
    public string Type { get; set; } = string.Empty;
    public MoneyDto Amount { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public string TraceNumber { get; set; } = string.Empty;

    // Parameterless constructor for AutoMapper
    public TransactionDto() { }

    // Optional: Constructor for manual creation
    public TransactionDto(
        Guid id,
        string friendlyId,
        Guid walletId,
        string type,
        MoneyDto amount,
        string status,
        DateTime timestamp,
        string reference,
        string description,
        string failureReason,
        string traceNumber)
    {
        Id = id;
        FriendlyId = friendlyId;
        WalletId = walletId;
        Type = type;
        Amount = amount;
        Status = status;
        Timestamp = timestamp;
        Reference = reference;
        Description = description;
        FailureReason = failureReason;
        TraceNumber = traceNumber;
    }
}
