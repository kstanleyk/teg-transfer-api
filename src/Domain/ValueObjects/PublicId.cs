namespace Agrovet.Domain.ValueObjects;

public sealed record PublicId
{
    public Guid Value { get; init; }

    private PublicId(Guid value) => Value = value;

    public static PublicId CreateUnique() => new(Guid.NewGuid());
    public static PublicId Create(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}