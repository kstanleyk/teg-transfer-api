using Transfer.Domain.Exceptions;

namespace Transfer.Domain.ValueObjects;

public class Percentage
{
    private readonly decimal _value;

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new DomainException("Percentage must be between 0-100");

        _value = value;
    }

    public decimal Value => _value;

    public static implicit operator decimal(Percentage p) => p._value;
}