using Microsoft.AspNetCore.Identity;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Client : IdentityUser<Guid>
{
    // Custom properties
    public string FirstName { get; private init; }
    public string LastName { get; private init; }
    public DateTime CreatedAt { get; private init; }
    public ClientStatus Status { get; private set; }

    // Client Group relationship
    public Guid? ClientGroupId { get; private set; }
    public ClientGroup? ClientGroup { get; private set; }

    public Wallet Wallet { get; private set; } = null!;

    // Private constructor for EF Core
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
        ClientGroup? clientGroup = null,
        DateTime? createdAt = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);
        DomainGuards.AgainstNullOrWhiteSpace(firstName);
        DomainGuards.AgainstNullOrWhiteSpace(lastName);

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

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
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = ClientStatus.Active,
            EmailConfirmed = false
        };

        // Assign to group if provided
        if (clientGroup != null)
        {
            client.ClientGroupId = clientGroup.Id;
            client.ClientGroup = clientGroup;
        }

        // Create wallet
        client.Wallet = Wallet.Create(clientId, currency, client.CreatedAt);

        return client;
    }

    // Group management methods
    public void AssignToGroup(ClientGroup clientGroup, string reason = "Group assignment")
    {
        DomainGuards.AgainstNull(clientGroup, nameof(clientGroup));

        if (ClientGroupId == clientGroup.Id)
            return;

        if (!clientGroup.IsActive)
            throw new DomainException("Cannot assign client to inactive group");

        if (Status == ClientStatus.Suspended)
            throw new DomainException("Cannot assign group to suspended client");

        ClientGroupId = clientGroup.Id;
        ClientGroup = clientGroup;
    }

    public void RemoveFromGroup(string reason = "Removed from group")
    {
        if (ClientGroupId == null) return;

        ClientGroupId = null;
        ClientGroup = null;
    }

    public void UpdateGroup(ClientGroup? newClientGroup, string reason = "Group updated")
    {
        if (newClientGroup == null)
        {
            RemoveFromGroup(reason);
        }
        else
        {
            AssignToGroup(newClientGroup, reason);
        }
    }

    // Existing methods
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
        if (Status == ClientStatus.Suspended) return;
        Status = ClientStatus.Suspended;
    }

    public void Activate(string reason = "Client activated")
    {
        if (Status == ClientStatus.Active) return;
        Status = ClientStatus.Active;
    }

    public void Deactivate(string reason = "Client deactivated")
    {
        if (Status == ClientStatus.Inactive) return;
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
               ClientGroupId != other.ClientGroupId;
    }

    public string FullName => $"{FirstName} {LastName}";

    // Validation methods
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
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
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 10;
    }
}
