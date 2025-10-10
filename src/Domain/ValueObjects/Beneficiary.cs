namespace Transfer.Domain.ValueObjects;

public class Beneficiary
{
    public string Name { get; }
    public string Relation { get; }
    public Percentage Percentage { get; }

    public Beneficiary(string name, string relation, Percentage percentage)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Relation = relation;
        Percentage = percentage;
    }
}