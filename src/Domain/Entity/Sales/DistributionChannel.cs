using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Sales;

public class DistributionChannel : Entity<string>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    protected DistributionChannel()
    {
    }

    public static DistributionChannel Create(string name, string? description = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);

        return new DistributionChannel
        {
            Name = name,
            Description = description,
            IsActive = true,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void Update(DistributionChannel distributionChannel)
    {
        DomainGuards.AgainstNullOrWhiteSpace(distributionChannel.Name);

        Name = distributionChannel.Name;
        Description = distributionChannel.Description;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public bool HasChanges(DistributionChannel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name ||
               Description != other.Description ||
               IsActive != other.IsActive;
    }
}
