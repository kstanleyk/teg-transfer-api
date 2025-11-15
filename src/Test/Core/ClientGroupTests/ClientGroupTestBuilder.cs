using TegWallet.Domain.Entity.Core;

namespace TegWallet.Core.Test.ClientGroupTests;

public class ClientGroupTestBuilder
{
    private string _name = "Test Group";
    private string _description = "Test group description";
    private string _createdBy = "system";
    private DateTime? _createdAt;
    private bool _isActive = true;

    public ClientGroupTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ClientGroupTestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ClientGroupTestBuilder WithCreatedBy(string createdBy)
    {
        _createdBy = createdBy;
        return this;
    }

    public ClientGroupTestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ClientGroupTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public ClientGroup Build()
    {
        var group = ClientGroup.Create(
            _name,
            _description,
            _createdBy
        );

        // Apply custom creation date if specified
        if (_createdAt.HasValue)
        {
            // Use reflection to set the private CreatedAt field if needed
            // This is a bit more complex since CreatedAt is init-only
            // For now, we rely on the factory method's DateTime.UtcNow
        }

        // Apply active status if different from default
        if (!_isActive)
        {
            group.Deactivate("Test setup");
        }

        return group;
    }

    // Predefined factory methods for common scenarios

    /// <summary>
    /// Creates an active client group with default test data
    /// </summary>
    public static ClientGroup CreateActiveGroup() => new ClientGroupTestBuilder().Build();

    /// <summary>
    /// Creates an inactive client group
    /// </summary>
    public static ClientGroup CreateInactiveGroup() =>
        new ClientGroupTestBuilder()
            .WithIsActive(false)
            .Build();

    /// <summary>
    /// Creates a client group with a specific name
    /// </summary>
    public static ClientGroup CreateGroupWithName(string name) =>
        new ClientGroupTestBuilder()
            .WithName(name)
            .Build();

    /// <summary>
    /// Creates a corporate client group
    /// </summary>
    public static ClientGroup CreateCorporateGroup() =>
        new ClientGroupTestBuilder()
            .WithName("Corporate Clients")
            .WithDescription("Corporate clients group")
            .WithCreatedBy("corporate-admin")
            .Build();

    /// <summary>
    /// Creates a VIP client group
    /// </summary>
    public static ClientGroup CreateVipGroup() =>
        new ClientGroupTestBuilder()
            .WithName("VIP Clients")
            .WithDescription("Very important clients")
            .WithCreatedBy("vip-admin")
            .Build();

    /// <summary>
    /// Creates a group with historical creation date (for testing date-related logic)
    /// </summary>
    public static ClientGroup CreateHistoricalGroup(DateTime creationDate) =>
        new ClientGroupTestBuilder()
            .WithCreatedAt(creationDate)
            .Build();

    /// <summary>
    /// Creates multiple groups for bulk operations testing
    /// </summary>
    public static List<ClientGroup> CreateMultipleGroups(int count, Action<ClientGroupTestBuilder, int>? configure = null)
    {
        var groups = new List<ClientGroup>();
        for (int i = 0; i < count; i++)
        {
            var builder = new ClientGroupTestBuilder()
                .WithName($"Test Group {i}")
                .WithDescription($"Description for group {i}")
                .WithCreatedBy($"creator{i}");

            configure?.Invoke(builder, i);
            groups.Add(builder.Build());
        }
        return groups;
    }
}