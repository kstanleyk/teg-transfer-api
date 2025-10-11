namespace TegWallet.Domain.Abstractions;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    public void SetId(TId id)
    {
        Id = id;
    }
}

