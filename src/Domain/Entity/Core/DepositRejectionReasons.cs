namespace TegWallet.Domain.Entity.Core;

public static class DepositRejectionReasons
{
    public const string BankTransferNotVerified = "Bank transfer could not be verified";
    public const string InsufficientFunds = "Insufficient funds in source account";
    public const string SuspiciousActivity = "Suspicious activity detected";
    public const string InvalidReference = "Invalid or missing reference number";
    public const string AccountFrozen = "Source account is frozen or restricted";
    public const string DocumentationRequired = "Additional documentation required";
    public const string AmountMismatch = "Amount does not match expected value";
    public const string DuplicateTransaction = "Duplicate transaction detected";

    public static readonly string[] AllReasons =
    [
        BankTransferNotVerified,
        InsufficientFunds,
        SuspiciousActivity,
        InvalidReference,
        AccountFrozen,
        DocumentationRequired,
        AmountMismatch,
        DuplicateTransaction
    ];
}