namespace Transfer.Domain.Entity.Sales;

public class PriceItem
{
    public string ChannelId { get; private set; } = null!;
    public string ItemId { get; private set; } = null!;
    public double Amount { get; private set; }
    public DateTime EffectiveDate { get; private set; }

    protected PriceItem()
    {
    }

    public static PriceItem Create(
        string channelId,
        string itemId,
        double amount,
        DateTime effectiveDate)
    {
        DomainGuards.AgainstNullOrWhiteSpace(channelId);
        DomainGuards.AgainstNullOrWhiteSpace(itemId);

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new PriceItem
        {
            ChannelId = channelId,
            ItemId = itemId,
            Amount = amount,
            EffectiveDate = effectiveDate,
        };
    }

    public void Update(PriceItem other)
    {
        DomainGuards.AgainstNullOrWhiteSpace(other.ChannelId);
        DomainGuards.AgainstNullOrWhiteSpace(other.ItemId);

        if (other.Amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(other.Amount));

        ChannelId = other.ChannelId;
        ItemId = other.ItemId;
        Amount = other.Amount;
        EffectiveDate = other.EffectiveDate;
    }

    public bool HasChanges(PriceItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Math.Abs(Amount - other.Amount) > 0.001;
    }
}