using Transfer.Domain.Abstractions;
using Transfer.Domain.Entity.Enum;
using Transfer.Domain.Exceptions;
using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity.Core;

public class Client : Entity<Guid>
{
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string FirstName { get; private init; } = null!;
    public string LastName { get; private init; } = null!;
    public DateTime CreatedAt { get; private init; }
    public ClientStatus Status { get; private set; }

    public Wallet Wallet { get; private set; } = null!;

    // Private constructor for EF Core and internal operations
    protected Client()
    {
    }

    public static Client Create(string email, string phoneNumber, string firstName, string lastName,
        Currency? defaultCurrency = null, DateTime? createdAt = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);
        DomainGuards.AgainstNullOrWhiteSpace(firstName);
        DomainGuards.AgainstNullOrWhiteSpace(lastName);

        // Validate email format
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        // Validate phone number format (basic check)
        if (!IsValidPhoneNumber(phoneNumber))
            throw new DomainException("Invalid phone number format");

        var clientId = Guid.NewGuid();
        var currency = defaultCurrency ?? Currency.XOF; // Default to XOF if not specified

        var client = new Client
        {
            Id = clientId,
            Email = email.Trim().ToLower(),
            PhoneNumber = phoneNumber.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = ClientStatus.Active
        };

        // Create wallet associated with the client
        client.Wallet = Wallet.Create(clientId, currency, client.CreatedAt);

        return client;
    }

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        if (!IsValidPhoneNumber(phoneNumber))
            throw new DomainException("Invalid phone number format");

        //var oldEmail = Email;
        //var oldPhone = PhoneNumber;

        Email = email.Trim().ToLower();
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
               Status != other.Status;
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