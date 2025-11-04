using Microsoft.AspNetCore.Identity;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Client : IdentityUser<Guid>
{
    public string FirstName { get; private init; }
    public string LastName { get; private init; }
    public DateTime CreatedAt { get; private init; }
    public ClientStatus Status { get; private set; }

    public string? ClientGroup { get; private set; }

    public Wallet Wallet { get; private set; } = null!;

    // Private constructor for EF Core and internal operations
    protected Client()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public static Client Create(
        string email,
        string phoneNumber,
        string firstName,
        string lastName,
        Currency? defaultCurrency = null,
        string? clientGroup = null,  // Add clientGroup parameter
        DateTime? createdAt = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);
        DomainGuards.AgainstNullOrWhiteSpace(firstName);
        DomainGuards.AgainstNullOrWhiteSpace(lastName);

        // Validate email format
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        // Validate phone number format
        if (!IsValidPhoneNumber(phoneNumber))
            throw new DomainException("Invalid phone number format");

        var clientId = Guid.NewGuid();
        var currency = defaultCurrency ?? Currency.XOF;

        var client = new Client
        {
            Id = clientId,
            UserName = email.Trim().ToLower(),
            NormalizedUserName = email.Trim().ToUpperInvariant(),
            Email = email.Trim().ToLower(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            ClientGroup = clientGroup?.Trim(), // Set client group
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = ClientStatus.Active,
            EmailConfirmed = false
        };

        // Create wallet associated with the client
        client.Wallet = Wallet.Create(clientId, currency, client.CreatedAt);

        return client;
    }

    /// <summary>
    /// Assigns the client to a specific group for exchange rate targeting
    /// Groups allow applying different exchange rates to categories of clients (e.g., VIP, Corporate, Retail)
    /// </summary>
    /// <param name="clientGroup">The group to assign the client to</param>
    /// <param name="reason">Reason for the group change (for audit purposes)</param>
    public void AssignToGroup(string clientGroup, string reason = "Group assignment")
    {
        DomainGuards.AgainstNullOrWhiteSpace(clientGroup, nameof(clientGroup));

        if (ClientGroup == clientGroup.Trim())
            return; // Already in the same group

        // Validate group name format (alphanumeric and hyphens/underscores)
        if (!IsValidGroupName(clientGroup))
            throw new DomainException("Group name can only contain letters, numbers, hyphens, and underscores");

        // You could add business rules here, e.g.:
        // - Prevent group changes for suspended clients
        // - Validate group exists in system
        // - Check permissions for group assignment

        var previousGroup = ClientGroup;
        ClientGroup = clientGroup.Trim();

        // Domain event could be raised here if needed
        // AddDomainEvent(new ClientGroupChangedDomainEvent(Id, previousGroup, ClientGroup, reason));
    }

    /// <summary>
    /// Removes the client from their current group
    /// Client will then use general exchange rates
    /// </summary>
    /// <param name="reason">Reason for removing from group</param>
    public void RemoveFromGroup(string reason = "Removed from group")
    {
        if (string.IsNullOrEmpty(ClientGroup))
            return; // Already not in a group

        var previousGroup = ClientGroup;
        ClientGroup = null;

        // Domain event could be raised here if needed
        // AddDomainEvent(new ClientGroupChangedDomainEvent(Id, previousGroup, null, reason));
    }

    /// <summary>
    /// Updates the client's group (combination of remove and assign)
    /// </summary>
    public void UpdateGroup(string? newClientGroup, string reason = "Group updated")
    {
        if (string.IsNullOrWhiteSpace(newClientGroup))
        {
            RemoveFromGroup(reason);
        }
        else
        {
            AssignToGroup(newClientGroup, reason);
        }
    }

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);

        if (!IsValidEmail(email)) throw new DomainException("Invalid email format");
        if (!IsValidPhoneNumber(phoneNumber)) throw new DomainException("Invalid phone number format");

        Email = email.Trim().ToLower();
        NormalizedEmail = email.Trim().ToUpperInvariant();
        UserName = Email;
        NormalizedUserName = Email.ToUpperInvariant();
        PhoneNumber = phoneNumber.Trim();
    }

    public void Suspend(string reason)
    {
        if (Status == ClientStatus.Suspended)
            return;

        //var previousStatus = Status;
        Status = ClientStatus.Suspended;
    }

    public void Activate(string reason = "Client activated")
    {
        if (Status == ClientStatus.Active)
            return;

        //var previousStatus = Status;
        Status = ClientStatus.Active;
    }

    public void Deactivate(string reason = "Client deactivated")
    {
        if (Status == ClientStatus.Inactive)
            return;

        //var previousStatus = Status;
        Status = ClientStatus.Inactive;
    }

    public bool HasChanges(Client? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Email != other.Email ||
               PhoneNumber != other.PhoneNumber ||
               FirstName != other.FirstName ||
               LastName != other.LastName ||
               Status != other.Status ||
               ClientGroup != other.ClientGroup; // Added ClientGroup comparison
    }

    private static bool IsValidGroupName(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return false;

        // Group names should be alphanumeric with hyphens/underscores
        // Adjust this regex based on your naming conventions
        return System.Text.RegularExpressions.Regex.IsMatch(groupName, @"^[a-zA-Z0-9_-]+$");
    }

    public string FullName => $"{FirstName} {LastName}";

    // Validation methods
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Basic phone number validation - can be enhanced based on requirements
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 10; // Minimum reasonable phone number length
    }
}