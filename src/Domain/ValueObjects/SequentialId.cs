using SequentialGuid;

namespace TegWallet.Domain.ValueObjects;

public sealed record SequentialId
{
    public Guid Value { get; init; }

    private SequentialId(Guid value) => Value = value;

    public static SequentialId CreateUnique() => new(SequentialGuidGenerator.Instance.NewGuid());
    public static SequentialId Create(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}