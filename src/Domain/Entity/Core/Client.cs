using System.Text.RegularExpressions;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Entity.Kyc;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Client : Entity<Guid>
{
    // Basic properties
    public string FirstName { get; private init; }
    public string LastName { get; private init; }
    public string Email { get; private init; } = string.Empty;
    public string PhoneNumber { get; private init; } = string.Empty;
    public DateTime CreatedAt { get; private init; }
    public ClientStatus Status { get; private set; }

    // KYC relationship
    public KycProfile? KycProfile { get; private set; }

    // Client Group relationship
    public Guid? ClientGroupId { get; private set; }
    public ClientGroup? ClientGroup { get; private set; }

    // Wallet relationship
    public Wallet Wallet { get; private set; } = null!;

    // User relationship
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

        // Initialize KYC profile
        client.InitializeKycProfile();

        return client;
    }

    // KYC Initialization
    private void InitializeKycProfile()
    {
        if (KycProfile != null)
            throw new DomainException("KYC profile already initialized");

        KycProfile = KycProfile.Create(Id);

        // Initialize email verification
        KycProfile.InitializeEmailVerification(Email);

        // Initialize phone verification
        KycProfile.InitializePhoneVerification(PhoneNumber);
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

    // Contact info update with KYC consideration
    public void UpdateContactInfo(string email, string phoneNumber)
    {
        DomainGuards.AgainstNullOrWhiteSpace(email);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        if (!IsValidPhoneNumber(phoneNumber))
            throw new DomainException("Invalid phone number format");

        // If email changed, reset email verification
        if (Email != email && KycProfile?.EmailVerification != null)
        {
            KycProfile.EmailVerification.Reset();
        }

        // If phone number changed, reset phone verification
        if (PhoneNumber != phoneNumber && KycProfile?.PhoneVerification != null)
        {
            KycProfile.PhoneVerification.Reset();
        }
    }

    // Status management with KYC integration
    public void Suspend(string reason)
    {
        if (Status == ClientStatus.Suspended)
            return;

        Status = ClientStatus.Suspended;

        // Also suspend KYC if active
        if (KycProfile?.IsVerified() == true)
        {
            KycProfile.Suspend("SYSTEM", $"Client suspended: {reason}");
        }
    }

    public void Activate(string reason = "Client activated")
    {
        if (Status == ClientStatus.Active)
            return;

        Status = ClientStatus.Active;

        // Reinstate KYC if it was suspended due to client suspension
        if (KycProfile?.IsSuspended() == true)
        {
            KycProfile.Reinstate("SYSTEM", $"Client activated: {reason}");
        }
    }

    public void Deactivate(string reason = "Client deactivated")
    {
        if (Status == ClientStatus.Inactive)
            return;

        Status = ClientStatus.Inactive;
    }

    // KYC Status Check and Sync
    public void CheckAndSyncKycStatus()
    {
        if (KycProfile == null)
            return;

        // Check KYC expiration
        KycProfile.CheckExpiration();

        // Sync client status with KYC status
        if (KycProfile.IsSuspended() && Status != ClientStatus.Suspended)
        {
            Status = ClientStatus.Suspended;
        }
        else if (KycProfile.IsVerified() && Status == ClientStatus.Suspended)
        {
            Status = ClientStatus.Active;
        }
    }

    // KYC Verification Methods
    public void VerifyEmailByCode(string verifiedBy = "SYSTEM")
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        KycProfile.VerifyEmailByCode();
    }

    public void VerifyPhoneBySms(string verifiedBy = "SYSTEM")
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        KycProfile.VerifyPhoneBySms();
    }

    public IdentityDocument AddIdentityDocument(
        KycDocumentType type,
        string documentNumber,
        DateTime issueDate,
        DateTime expiryDate,
        string? fullName = null,
        DateTime? dateOfBirth = null,
        string? nationality = null,
        string? issuingAuthority = null)
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        return KycProfile.AddIdentityDocument(
            type,
            documentNumber,
            issueDate,
            expiryDate,
            fullName,
            dateOfBirth,
            nationality,
            issuingAuthority);
    }

    public void SubmitIdentityDocumentForVerification(Guid documentId)
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        KycProfile.SubmitIdentityDocumentForVerification(documentId);
    }

    public void SubmitKycForReview()
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        KycProfile.SubmitForReview();
    }

    // KYC Status Checks
    public bool HasCompletedKycLevel1()
    {
        return KycProfile?.HasCompletedLevel1() ?? false;
    }

    public bool HasCompletedKycLevel2()
    {
        return KycProfile?.HasCompletedLevel2() ?? false;
    }

    public bool HasCompletedKycLevel3()
    {
        return KycProfile?.HasCompletedLevel3() ?? false;
    }

    // Transaction Eligibility based on KYC Levels
    public bool CanMakeDeposit()
    {
        return KycProfile?.CanMakeDeposit() ?? false;
    }

    public bool CanMakeStandardPurchase()
    {
        return KycProfile?.CanMakeStandardPurchase() ?? false;
    }

    public bool CanMakeLargePurchase(decimal amountThreshold = 500000) // Default threshold: 500,000 XAF
    {
        if (KycProfile == null)
            return false;

        return KycProfile.CanMakeLargePurchase();
    }

    public bool CanMakeInternationalTransfer()
    {
        return KycProfile?.CanMakeInternationalTransfer() ?? false;
    }

    public bool CanAccessAdvancedFeatures()
    {
        return KycProfile?.CanAccessAdvancedFeatures() ?? false;
    }

    // KYC Status Properties
    public bool IsKycVerified => KycProfile?.IsVerified() ?? false;
    public bool IsKycLevel1Verified => KycProfile?.IsLevel1Verified() ?? false;
    public bool IsKycLevel2Verified => KycProfile?.IsLevel2Verified() ?? false;
    public bool IsKycLevel3Verified => KycProfile?.IsLevel3Verified() ?? false;
    public bool IsKycPending => KycProfile?.IsPendingVerification() ?? false;
    public bool IsKycUnderReview => KycProfile?.IsUnderReview() ?? false;
    public bool IsKycRejected => KycProfile?.IsRejected() ?? false;
    public bool IsKycExpired => KycProfile?.IsExpired() ?? false;
    public bool IsKycSuspended => KycProfile?.IsSuspended() ?? false;
    public bool IsKycNotStarted => KycProfile?.Status == KycStatus.NotStarted;

    // Utility Methods
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
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 10;
    }

    public void LinkToUser(Guid userId)
    {
        DomainGuards.AgainstDefault(userId, nameof(userId));

        UserId = userId;
    }

    // KYC Documents Access
    public IReadOnlyList<IdentityDocument> GetIdentityDocuments()
    {
        return KycProfile?.IdentityDocuments ?? new List<IdentityDocument>().AsReadOnly();
    }

    public IdentityDocument? GetIdentityDocument(Guid documentId)
    {
        return KycProfile?.IdentityDocuments.FirstOrDefault(d => d.Id == documentId);
    }

    public EmailVerification? GetEmailVerification()
    {
        return KycProfile?.EmailVerification;
    }

    public PhoneVerification? GetPhoneVerification()
    {
        return KycProfile?.PhoneVerification;
    }

    // KYC Expiry Information
    public DateTime? GetKycExpiryDate()
    {
        return KycProfile?.ExpiresAt;
    }

    public bool IsKycExpiringSoon(int daysThreshold = 30)
    {
        var expiryDate = KycProfile?.ExpiresAt;
        if (!expiryDate.HasValue || KycProfile?.IsVerified() != true)
            return false;

        return expiryDate.Value <= DateTime.UtcNow.AddDays(daysThreshold)
            && expiryDate.Value > DateTime.UtcNow;
    }

    // KYC Verification History
    public IReadOnlyList<KycVerificationHistory> GetKycVerificationHistory()
    {
        return KycProfile?.VerificationHistory ?? new List<KycVerificationHistory>().AsReadOnly();
    }

    // Compliance Checks
    public bool IsCompliantForTransaction(decimal amount, TransactionType transactionType, string? destinationCountry = null)
    {
        // Basic compliance check
        if (!IsKycVerified)
            return false;

        // Amount-based checks
        switch (transactionType)
        {
            case TransactionType.Deposit:
                return CanMakeDeposit();

            case TransactionType.Withdrawal:
                return CanMakeDeposit(); // Same as deposit for withdrawals

            case TransactionType.Purchase:
                if (amount <= 50000) // Small purchases
                    return CanMakeStandardPurchase();
                else if (amount <= 500000) // Medium purchases
                    return CanMakeLargePurchase();
                else // Large purchases
                    return CanAccessAdvancedFeatures();

            //case TransactionType.Transfer:
            //    if (!string.IsNullOrEmpty(destinationCountry) && destinationCountry != "CM") // Cameroon
            //        return CanMakeInternationalTransfer();
            //    return CanMakeStandardPurchase();

            default:
                return false;
        }
    }

    // KYC Approval/Rejection
    public void ApproveKyc(string verifiedBy, DateTime expiresAt, string? notes = null)
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        KycProfile.ApproveKyc(verifiedBy, expiresAt, notes);

        // Activate client if suspended
        if (Status == ClientStatus.Suspended)
        {
            Activate($"KYC approved: {notes}");
        }
    }

    public void RejectKyc(string rejectedBy, string reason)
    {
        if (KycProfile == null)
            throw new DomainException("KYC profile not initialized");

        KycProfile.RejectKyc(rejectedBy, reason);

        // Suspend client if KYC is rejected
        if (Status == ClientStatus.Active)
        {
            Suspend($"KYC rejected: {reason}");
        }
    }
}

//public class Client : Entity<Guid>
//{
//    // Custom properties
//    public string FirstName { get; private init; }
//    public string LastName { get; private init; }
//    public string Email { get; private init; } = string.Empty;
//    public string PhoneNumber { get; private init; } = string.Empty;
//    public DateTime CreatedAt { get; private init; }
//    public ClientStatus Status { get; private set; }

//    // Client Group relationship
//    public Guid? ClientGroupId { get; private set; }
//    public ClientGroup? ClientGroup { get; private set; }

//    public Wallet Wallet { get; private set; } = null!;

//    public virtual ApplicationUser? User { get; private set; }
//    public Guid? UserId { get; private set; }

//    // Private constructor for EF Core
//    protected Client()
//    {
//        FirstName = string.Empty;
//        LastName = string.Empty;
//    }

//    public static Client Create(
//        string email,
//        string phoneNumber,
//        string firstName,
//        string lastName,
//        Currency? defaultCurrency = null,
//        ClientGroup? clientGroup = null,
//        DateTime? createdAt = null)
//    {
//        DomainGuards.AgainstNullOrWhiteSpace(email);
//        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);
//        DomainGuards.AgainstNullOrWhiteSpace(firstName);
//        DomainGuards.AgainstNullOrWhiteSpace(lastName);

//        if (!IsValidEmail(email))
//            throw new DomainException("Invalid email format");

//        if (!IsValidPhoneNumber(phoneNumber))
//            throw new DomainException("Invalid phone number format");

//        var clientId = Guid.NewGuid();
//        var currency = defaultCurrency ?? Currency.XAF;

//        var client = new Client
//        {
//            Id = clientId,
//            Email = email.Trim(),
//            PhoneNumber = phoneNumber.Trim(),
//            FirstName = firstName.Trim(),
//            LastName = lastName.Trim(),
//            CreatedAt = createdAt ?? DateTime.UtcNow,
//            Status = ClientStatus.Active
//        };

//        // Assign to group if provided
//        if (clientGroup != null)
//        {
//            client.ClientGroupId = clientGroup.Id;
//            client.ClientGroup = clientGroup;
//        }

//        // Create wallet
//        client.Wallet = Wallet.Create(clientId, currency, client.CreatedAt);

//        return client;
//    }

//    // Group management methods
//    public void AssignToGroup(ClientGroup clientGroup, string reason = "Group assignment")
//    {
//        DomainGuards.AgainstNull(clientGroup, nameof(clientGroup));

//        if (ClientGroupId == clientGroup.Id)
//            return;

//        if (!clientGroup.IsActive)
//            throw new DomainException("Cannot assign client to inactive group");

//        if (Status == ClientStatus.Suspended)
//            throw new DomainException("Cannot assign group to suspended client");

//        ClientGroupId = clientGroup.Id;
//        //ClientGroup = clientGroup;
//    }

//    public void RemoveFromGroup(string reason = "Removed from group")
//    {
//        if (ClientGroupId == null) return;

//        ClientGroupId = null;
//        ClientGroup = null;
//    }

//    public void UpdateGroup(ClientGroup? newClientGroup, string reason = "Group updated")
//    {
//        if (newClientGroup == null)
//        {
//            RemoveFromGroup(reason);
//        }
//        else
//        {
//            AssignToGroup(newClientGroup, reason);
//        }
//    }

//    public void UpdateContactInfo(string email, string phoneNumber)
//    {
//        DomainGuards.AgainstNullOrWhiteSpace(email);
//        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);

//        if (!IsValidEmail(email)) throw new DomainException("Invalid email format");
//        if (!IsValidPhoneNumber(phoneNumber)) throw new DomainException("Invalid phone number format");
//    }

//    public void Suspend(string reason)
//    {
//        if (Status == ClientStatus.Suspended) return;
//        Status = ClientStatus.Suspended;
//    }

//    public void Activate(string reason = "Client activated")
//    {
//        if (Status == ClientStatus.Active) return;
//        Status = ClientStatus.Active;
//    }

//    public void Deactivate(string reason = "Client deactivated")
//    {
//        if (Status == ClientStatus.Inactive) return;
//        Status = ClientStatus.Inactive;
//    }

//    public bool HasChanges(Client? other)
//    {
//        if (other is null) return false;
//        if (ReferenceEquals(this, other)) return false;

//        return FirstName != other.FirstName ||
//               LastName != other.LastName ||
//               Status != other.Status ||
//               ClientGroupId != other.ClientGroupId;
//    }

//    public string FullName => $"{FirstName} {LastName}";

//    public static bool IsValidEmail(string email)
//    {
//        if (string.IsNullOrWhiteSpace(email))
//            return false;

//        try
//        {
//            var addr = new System.Net.Mail.MailAddress(email);
//            email = addr.Address;
//        }
//        catch
//        {
//            return false;
//        }

//        const string pattern =
//            @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";

//        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
//    }


//    private static bool IsValidPhoneNumber(string phoneNumber)
//    {
//        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
//        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
//        return cleaned.Length >= 10;
//    }

//    public void LinkToUser(Guid userId)
//    {
//        DomainGuards.AgainstDefault(userId, nameof(userId));

//        UserId = userId;
//    }
//}
