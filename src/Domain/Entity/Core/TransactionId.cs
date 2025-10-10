namespace Transfer.Domain.Entity.Core;

public record TransactionId(Guid Value)
{
    public static TransactionId New() => new(Guid.NewGuid());
}