using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Domain.Entity.Core;

public class ClientGroup : Entity<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public string CreatedBy { get; private init; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Navigation properties
    private readonly List<Client> _clients = [];
    public IReadOnlyList<Client> Clients => _clients.AsReadOnly();

    private readonly List<ExchangeRate> _exchangeRates = [];
    public IReadOnlyList<ExchangeRate> ExchangeRates => _exchangeRates.AsReadOnly();

    // Private constructor for EF Core
    private ClientGroup()
    {
        Name = string.Empty;
        Description = string.Empty;
        CreatedBy = string.Empty;
    }

    public static ClientGroup Create(string name, string description, string createdBy)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(createdBy, nameof(createdBy));

        ValidateGroupName(name);

        return new ClientGroup
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy.Trim()
        };
    }

    public void Update(string name, string description, string updatedBy)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(updatedBy, nameof(updatedBy));

        ValidateGroupName(name);

        Name = name.Trim();
        Description = description.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy.Trim();
    }

    public void Deactivate(string deactivatedBy)
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deactivatedBy;
    }

    public void Activate(string activatedBy)
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = activatedBy;
    }

    public bool CanBeDeleted() => !_clients.Any();

    private static void ValidateGroupName(string name)
    {
        if (name.Length < 2 || name.Length > 50)
            throw new DomainException("Group name must be between 2 and 50 characters");

        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s_-]+$"))
            throw new DomainException("Group name can only contain letters, numbers, spaces, hyphens, and underscores");
    }
}