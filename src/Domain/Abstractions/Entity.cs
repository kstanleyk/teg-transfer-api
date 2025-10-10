namespace Transfer.Domain.Abstractions;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    public Guid PublicId { get; protected set; }
    public DateTime CreatedOn { get; protected set; }

    public void SetPublicId(Guid publicId)
    {
        PublicId = publicId;
    }

    public void SetId(TId id)
    {
        Id = id;
    }

    public void SetCreatedOn(DateTime createdOn)
    {
        CreatedOn = createdOn;
    }
}

