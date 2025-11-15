using System.Text.RegularExpressions;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Client : Entity<Guid>
{
    // Custom properties
    public string FirstName { get; private init; }
    public string LastName { get; private init; }
    public string Email { get; private init; } = string.Empty;
    public string PhoneNumber { get; private init; } = string.Empty;
    public DateTime CreatedAt { get; private init; }
    public ClientStatus Status { get; private set; }

    // Client Group relationship
    public Guid? ClientGroupId { get; private set; }
    public ClientGroup? ClientGroup { get; private set; }

    public Wallet Wallet { get; private set; } = null!;

    public virtual ApplicationUser? User { get; private set; }
    public Guid? UserId { get; private set; }

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
        var currency = defaultCurrency ?? Currency.XAF;

        var client = new Client
        {
            Id = clientId,
            Email = email.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = ClientStatus.Active
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
        //ClientGroup = clientGroup;
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

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);

        if (!IsValidEmail(email)) throw new DomainException("Invalid email format");
        if (!IsValidPhoneNumber(phoneNumber)) throw new DomainException("Invalid phone number format");
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

        return FirstName != other.FirstName ||
               LastName != other.LastName ||
               Status != other.Status ||
               ClientGroupId != other.ClientGroupId;
    }

    public string FullName => $"{FirstName} {LastName}";

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            email = addr.Address;
        }
        catch
        {
            return false;
        }

        const string pattern =
            @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";

        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }


    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 10;
    }

    public void LinkToUser(Guid userId)
    {
        DomainGuards.AgainstDefault(userId, nameof(userId));

        UserId = userId;
    }
}
