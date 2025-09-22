namespace Agrovet.Domain.Abstractions;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    public Guid PublicId { get; protected set; }
    public DateTime CreatedOn { get; protected set; }

    public void SetPublicId(Guid publicId)
    {
        PublicId = publicId;
    }

    public void SetCreatedOn(DateTime createdOn)
    {
        CreatedOn = createdOn;
    }
}

public abstract class AuthEntity<TId>
{
    public TId Id { get; protected set; } = default!;
}
