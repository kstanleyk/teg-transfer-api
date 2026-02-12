using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Kyc;

public enum KycStatus
{
    NotStarted = 1,
    Level1Pending = 2,      // Basic info verification (email/phone)
    Level1Verified = 3,
    Level2Pending = 4,      // Identity document verification
    Level2Verified = 5,
    Level3Pending = 6,      // Enhanced verification (third-party, biometric, etc.)
    Level3Verified = 7,
    UnderReview = 8,
    Rejected = 9,
    Suspended = 10,
    Expired = 11
}

public enum KycVerificationMethod
{
    SelfService = 1,        // User uploaded/entered
    AdminVerified = 2,      // Admin manually verified
    ThirdPartyApi = 3,      // External provider (Jumio, Onfido, etc.)
    AutomatedSystem = 4,    // OCR, facial recognition, etc.
    BankStatement = 5       // Bank account verification
}

public enum KycVerificationStatus
{
    Pending = 1,
    Submitted = 2,
    Verified = 3,
    Failed = 4,
    Rejected = 5,
    Expired = 6
}

public enum KycDocumentType
{
    Passport = 1,
    NationalId = 2,
    DriversLicense = 3,
    VotersCard = 4,
    BirthCertificate = 5,
    UtilityBill = 6,
    BankStatement = 7,
    TaxCertificate = 8,
    BusinessRegistration = 9,
    ProofOfAddress = 10,
    SelfiePhoto = 11,       // For facial verification
    Other = 99
}

// Value Object for verification details
public class KycVerificationDetails : ValueObject
{
    public KycVerificationMethod Method { get; private set; }
    public string VerifiedBy { get; private set; } = string.Empty;
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationId { get; private set; } // External provider ID
    public string? ProviderName { get; private set; }   // Jumio, Onfido, etc.
    public string? Notes { get; private set; }

    public KycVerificationDetails() { }

    public KycVerificationDetails(
        KycVerificationMethod method,
        string verifiedBy,
        DateTime verifiedAt,
        string? verificationId = null,
        string? providerName = null,
        string? notes = null)
    {
        Method = method;
        VerifiedBy = verifiedBy.Trim();
        VerifiedAt = verifiedAt;
        VerificationId = verificationId?.Trim();
        ProviderName = providerName?.Trim();
        Notes = notes?.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Method;
        yield return VerifiedBy;
        yield return VerifiedAt;
        yield return VerificationId ?? string.Empty;
        yield return ProviderName ?? string.Empty;
        yield return Notes ?? string.Empty;
    }
}

// Value Object for verification attempt
public class KycVerificationAttempt : ValueObject
{
    public DateTime AttemptedAt { get; private set; }
    public KycVerificationMethod Method { get; private set; }
    public bool Successful { get; private set; }
    public string? FailureReason { get; private set; }
    public string? ReferenceId { get; private set; }

    public KycVerificationAttempt(
        DateTime attemptedAt,
        KycVerificationMethod method,
        bool successful,
        string? failureReason = null,
        string? referenceId = null)
    {
        AttemptedAt = attemptedAt;
        Method = method;
        Successful = successful;
        FailureReason = failureReason?.Trim();
        ReferenceId = referenceId?.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AttemptedAt;
        yield return Method;
        yield return Successful;
        yield return FailureReason ?? string.Empty;
        yield return ReferenceId ?? string.Empty;
    }
}

// Email Verification Entity
public class EmailVerification : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public string Email { get; private set; }
    public KycVerificationStatus Status { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }

    // Verification details
    private KycVerificationDetails? _verificationDetails;
    public KycVerificationDetails? VerificationDetails => _verificationDetails;

    // Verification attempts history
    private readonly List<KycVerificationAttempt> _attempts = [];
    public IReadOnlyList<KycVerificationAttempt> Attempts => _attempts.AsReadOnly();

    // Private constructor for EF Core
    private EmailVerification()
    {
        Email = string.Empty;
    }

    public static EmailVerification Create(Guid clientId, string email)
    {
        DomainGuards.AgainstDefault(clientId);
        DomainGuards.AgainstNullOrWhiteSpace(email);

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        return new EmailVerification
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Email = email.Trim().ToLower(),
            Status = KycVerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsSubmitted()
    {
        if (Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot submit verification from status: {Status}");

        Status = KycVerificationStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            KycVerificationMethod.SelfService,
            true));
    }

    public void VerifyByCode(string verifiedBy = "SYSTEM")
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.SelfService,
            verifiedBy,
            DateTime.UtcNow,
            notes: "Verified by confirmation code");
    }

    public void VerifyByAdmin(string adminEmail, string? notes = null)
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.AdminVerified,
            adminEmail,
            DateTime.UtcNow,
            notes: notes);
    }

    public void MarkAsFailed(string reason)
    {
        Status = KycVerificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            KycVerificationMethod.SelfService,
            false,
            reason));
    }

    public void Reset()
    {
        Status = KycVerificationStatus.Pending;
        VerifiedAt = null;
        _verificationDetails = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsVerified => Status == KycVerificationStatus.Verified;
    public bool IsPending => Status == KycVerificationStatus.Pending || Status == KycVerificationStatus.Submitted;

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
}

// Phone Verification Entity
public class PhoneVerification : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public string PhoneNumber { get; private set; }
    public KycVerificationStatus Status { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }

    // Verification details
    private KycVerificationDetails? _verificationDetails;
    public KycVerificationDetails? VerificationDetails => _verificationDetails;

    // Verification attempts history
    private readonly List<KycVerificationAttempt> _attempts = [];
    public IReadOnlyList<KycVerificationAttempt> Attempts => _attempts.AsReadOnly();

    // Private constructor for EF Core
    private PhoneVerification()
    {
        PhoneNumber = string.Empty;
    }

    public static PhoneVerification Create(Guid clientId, string phoneNumber)
    {
        DomainGuards.AgainstDefault(clientId);
        DomainGuards.AgainstNullOrWhiteSpace(phoneNumber);

        if (!IsValidPhoneNumber(phoneNumber))
            throw new DomainException("Invalid phone number format");

        return new PhoneVerification
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            PhoneNumber = FormatPhoneNumber(phoneNumber),
            Status = KycVerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsSubmitted()
    {
        if (Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot submit verification from status: {Status}");

        Status = KycVerificationStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            KycVerificationMethod.SelfService,
            true));
    }

    public void VerifyBySms(string verifiedBy = "SYSTEM")
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.SelfService,
            verifiedBy,
            DateTime.UtcNow,
            notes: "Verified by SMS code");
    }

    public void VerifyByAdmin(string adminEmail, string? notes = null)
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.AdminVerified,
            adminEmail,
            DateTime.UtcNow,
            notes: notes);
    }

    public void MarkAsFailed(string reason)
    {
        Status = KycVerificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            KycVerificationMethod.SelfService,
            false,
            reason));
    }

    public void Reset()
    {
        Status = KycVerificationStatus.Pending;
        VerifiedAt = null;
        _verificationDetails = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsVerified => Status == KycVerificationStatus.Verified;
    public bool IsPending => Status == KycVerificationStatus.Pending || Status == KycVerificationStatus.Submitted;

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 10;
    }

    private static string FormatPhoneNumber(string phoneNumber)
    {
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned;
    }
}

// Identity Document Entity
public class IdentityDocument : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public KycDocumentType Type { get; private init; }
    public string DocumentNumber { get; private set; }
    public string? FullName { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Nationality { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public string? IssuingAuthority { get; private set; }
    public KycVerificationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }

    // Document files
    public string FrontImagePath { get; private set; } = string.Empty;
    public string? BackImagePath { get; private set; }
    public string? SelfieImagePath { get; private set; } // For facial matching

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletionReason { get; private set; }

    public void MarkAsDeleted(string reason)
    {
        if (Status == KycVerificationStatus.Verified)
            throw new DomainException("Cannot delete a verified document");

        if (Status == KycVerificationStatus.Submitted)
            throw new DomainException("Cannot delete a document that is submitted for verification");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    // Verification details
    private KycVerificationDetails? _verificationDetails;
    public KycVerificationDetails? VerificationDetails => _verificationDetails;

    // Verification attempts history
    private readonly List<KycVerificationAttempt> _attempts = [];
    public IReadOnlyList<KycVerificationAttempt> Attempts => _attempts.AsReadOnly();

    // Private constructor for EF Core
    private IdentityDocument()
    {
        DocumentNumber = string.Empty;
    }

    public static IdentityDocument Create(
        Guid clientId,
        KycDocumentType type,
        string documentNumber,
        DateTime issueDate,
        DateTime expiryDate,
        string? fullName = null,
        DateTime? dateOfBirth = null,
        string? nationality = null,
        string? issuingAuthority = null)
    {
        DomainGuards.AgainstDefault(clientId);
        DomainGuards.AgainstNullOrWhiteSpace(documentNumber);

        ValidateDocumentDates(issueDate, expiryDate);

        return new IdentityDocument
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Type = type,
            DocumentNumber = documentNumber.Trim(),
            FullName = fullName?.Trim(),
            DateOfBirth = dateOfBirth,
            Nationality = nationality?.Trim(),
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            IssuingAuthority = issuingAuthority?.Trim(),
            Status = KycVerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDocumentInfo(
        string documentNumber,
        DateTime issueDate,
        DateTime expiryDate,
        string? fullName = null,
        DateTime? dateOfBirth = null,
        string? nationality = null,
        string? issuingAuthority = null)
    {
        ValidateDocumentDates(issueDate, expiryDate);

        DocumentNumber = documentNumber.Trim();
        FullName = fullName?.Trim();
        DateOfBirth = dateOfBirth;
        Nationality = nationality?.Trim();
        IssueDate = issueDate;
        ExpiryDate = expiryDate;
        IssuingAuthority = issuingAuthority?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddFrontImage(string imagePath)
    {
        DomainGuards.AgainstNullOrWhiteSpace(imagePath);
        FrontImagePath = imagePath.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBackImage(string imagePath)
    {
        DomainGuards.AgainstNullOrWhiteSpace(imagePath);
        BackImagePath = imagePath.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSelfieImage(string imagePath)
    {
        DomainGuards.AgainstNullOrWhiteSpace(imagePath);
        SelfieImagePath = imagePath.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSubmitted()
    {
        if (Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot submit verification from status: {Status}");

        Status = KycVerificationStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            KycVerificationMethod.SelfService,
            true));
    }

    // Add to IdentityDocument domain model
    public void MarkAsUnderReview(string reviewedBy, string? notes = null)
    {
        if (Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot mark as under review from status: {Status}. Only pending documents can be marked under review.");

        if (string.IsNullOrEmpty(FrontImagePath))
            throw new DomainException("Document cannot be marked under review without a front image.");

        if (ExpiryDate < DateTime.UtcNow)
            throw new DomainException("Document has expired. Cannot mark as under review.");

        Status = KycVerificationStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            KycVerificationMethod.AdminVerified,
            true,
            referenceId: $"UNDER_REVIEW_{DateTime.UtcNow.Ticks}"));

        // Optionally store who marked it under review
        if (!string.IsNullOrEmpty(notes))
        {
            _verificationDetails = new KycVerificationDetails(
                KycVerificationMethod.AdminVerified,
                reviewedBy,
                DateTime.UtcNow,
                notes: $"Marked under review: {notes}");
        }
    }

    public void VerifyByAdmin(string adminEmail, string? notes = null)
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.AdminVerified,
            adminEmail,
            DateTime.UtcNow,
            notes: notes);
    }

    public void VerifyByThirdParty(
        string providerName,
        string verificationId,
        string? notes = null)
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.ThirdPartyApi,
            "SYSTEM",
            DateTime.UtcNow,
            verificationId,
            providerName,
            notes);
    }

    public void VerifyByAutomatedSystem(string? notes = null)
    {
        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Cannot verify from status: {Status}");

        Status = KycVerificationStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            KycVerificationMethod.AutomatedSystem,
            "SYSTEM",
            DateTime.UtcNow,
            notes: notes);
    }

    public void MarkAsFailed(string reason, KycVerificationMethod method = KycVerificationMethod.SelfService)
    {
        Status = KycVerificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            method,
            false,
            reason));
    }

    public void ResetVerification()
    {
        Status = KycVerificationStatus.Pending;
        _verificationDetails = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CheckExpiration()
    {
        if (Status == KycVerificationStatus.Verified && DateTime.UtcNow > ExpiryDate)
        {
            Status = KycVerificationStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Add this method to the IdentityDocument class
    public void Reject(string rejectedBy, string reason, KycVerificationMethod method = KycVerificationMethod.AdminVerified)
    {
        if (Status == KycVerificationStatus.Verified)
            throw new DomainException("Cannot reject a verified document. Consider suspending the KYC profile instead.");

        if (Status == KycVerificationStatus.Expired)
            throw new DomainException("Document is already expired. No need to reject it.");

        if (Status == KycVerificationStatus.Rejected)
            throw new DomainException("Document is already rejected.");

        if (Status == KycVerificationStatus.Failed)
            throw new DomainException("Document verification has already failed.");

        if (Status != KycVerificationStatus.Submitted && Status != KycVerificationStatus.Pending)
            throw new DomainException($"Document cannot be rejected in its current status: {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required");

        if (reason.Trim().Length < 10)
            throw new DomainException("Rejection reason must be at least 10 characters");

        Status = KycVerificationStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;

        _verificationDetails = new KycVerificationDetails(
            method,
            rejectedBy,
            DateTime.UtcNow,
            notes: $"Document rejected: {reason}");

        _attempts.Add(new KycVerificationAttempt(
            DateTime.UtcNow,
            method,
            false,
            reason,
            $"REJECT_{DateTime.UtcNow.Ticks}"));
    }

    public bool IsVerified => Status == KycVerificationStatus.Verified;
    public bool IsExpired => Status == KycVerificationStatus.Expired || DateTime.UtcNow > ExpiryDate;
    public bool IsValid => IsVerified && !IsExpired;

    private static void ValidateDocumentDates(DateTime issueDate, DateTime expiryDate)
    {
        if (issueDate > DateTime.UtcNow)
            throw new DomainException("Issue date cannot be in the future");

        if (expiryDate < DateTime.UtcNow)
            throw new DomainException("Document is already expired");

        if (expiryDate <= issueDate)
            throw new DomainException("Expiry date must be after issue date");
    }
}

// KYC Profile Aggregate Root with Levels
public class KycProfile : Aggregate<Guid>
{
    public Guid ClientId { get; private init; }
    public KycStatus Status { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? VerificationNotes { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }
    public string? VerifiedBy { get; private set; }
    public string? RejectedBy { get; private set; }

    // Navigation properties
    public EmailVerification? EmailVerification { get; private set; }
    public PhoneVerification? PhoneVerification { get; private set; }

    private readonly List<IdentityDocument> _identityDocuments = [];
    public IReadOnlyList<IdentityDocument> IdentityDocuments => _identityDocuments.AsReadOnly();

    private readonly List<KycVerificationHistory> _verificationHistory = [];
    public IReadOnlyList<KycVerificationHistory> VerificationHistory => _verificationHistory.AsReadOnly();

    // Private constructor for EF Core
    private KycProfile()
    {
    }

    public static KycProfile Create(Guid clientId)
    {
        DomainGuards.AgainstDefault(clientId);

        var profile = new KycProfile
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Status = KycStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return profile;
    }

    public void RemoveIdentityDocument(Guid documentId, string reason)
    {
        var document = _identityDocuments.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            throw new DomainException($"Document not found: {documentId}");

        if (document.Status == KycVerificationStatus.Verified)
            throw new DomainException("Cannot delete a verified document");

        if (document.Status == KycVerificationStatus.Submitted)
            throw new DomainException("Cannot delete a document that is submitted for verification");

        _identityDocuments.Remove(document);
        UpdatedAt = DateTime.UtcNow;
    }

    // Email Verification Management
    public void InitializeEmailVerification(string email)
    {
        if (EmailVerification != null)
            throw new DomainException("Email verification already initialized");

        EmailVerification = EmailVerification.Create(ClientId, email);
        TryAdvanceLevel();
    }

    public void VerifyEmailByCode()
    {
        if (EmailVerification == null)
            throw new DomainException("Email verification not initialized");

        EmailVerification.VerifyByCode();
        TryAdvanceLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmailByAdmin(string adminEmail, string? notes = null)
    {
        if (EmailVerification == null)
            throw new DomainException("Email verification not initialized");

        EmailVerification.VerifyByAdmin(adminEmail, notes);
        TryAdvanceLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    // Phone Verification Management
    public void InitializePhoneVerification(string phoneNumber)
    {
        if (PhoneVerification != null)
            throw new DomainException("Phone verification already initialized");

        PhoneVerification = PhoneVerification.Create(ClientId, phoneNumber);
        TryAdvanceLevel();
    }

    public void VerifyPhoneBySms()
    {
        if (PhoneVerification == null)
            throw new DomainException("Phone verification not initialized");

        PhoneVerification.VerifyBySms();
        TryAdvanceLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyPhoneByAdmin(string adminEmail, string? notes = null)
    {
        if (PhoneVerification == null)
            throw new DomainException("Phone verification not initialized");

        PhoneVerification.VerifyByAdmin(adminEmail, notes);
        TryAdvanceLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    // Identity Document Management
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
        // Check if similar document already exists
        var existingDocument = _identityDocuments
            .FirstOrDefault(d => d.Type == type &&
                                d.DocumentNumber == documentNumber &&
                                d.Status != KycVerificationStatus.Expired);

        if (existingDocument != null)
            throw new DomainException($"Document of type {type} with number {documentNumber} already exists");

        var document = IdentityDocument.Create(
            ClientId,
            type,
            documentNumber,
            issueDate,
            expiryDate,
            fullName,
            dateOfBirth,
            nationality,
            issuingAuthority);

        _identityDocuments.Add(document);

        // Update KYC status to Level 2 Pending if not already there
        if (Status == KycStatus.Level1Verified)
        {
            Status = KycStatus.Level2Pending;
        }

        UpdatedAt = DateTime.UtcNow;

        return document;
    }

    public void ApproveLevel1(string approvedBy, DateTime expiresAt, string? notes = null)
    {
        // Business logic validation
        if (Status == KycStatus.Level1Verified)
            throw new DomainException("KYC Level 1 is already verified.");

        if (Status == KycStatus.Level2Verified || Status == KycStatus.Level3Verified)
            throw new DomainException($"Cannot approve Level 1 when already at Level {GetCurrentKycLevel()}.");

        if (Status == KycStatus.Rejected || Status == KycStatus.Suspended || Status == KycStatus.Expired)
            throw new DomainException($"Cannot approve Level 1 for KYC profile in status: {Status}");

        if (!HasCompletedLevel1())
            throw new DomainException("Level 1 requirements not met. Email and phone must be verified.");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        if (expiresAt > DateTime.UtcNow.AddYears(2))
            throw new DomainException("Expiration date cannot be more than 2 years in the future.");

        var previousStatus = Status;
        Status = KycStatus.Level1Verified;
        VerifiedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        VerificationNotes = notes;
        VerifiedBy = approvedBy;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            previousStatus,
            Status,
            "Level 1 KYC approved",
            approvedBy,
            notes));
    }

    public void ApproveLevel2(string approvedBy, DateTime expiresAt, string? notes = null)
    {
        // Business logic validation
        if (Status == KycStatus.Level2Verified)
            throw new DomainException("KYC Level 2 is already verified.");

        if (Status == KycStatus.Level3Verified)
            throw new DomainException("Cannot approve Level 2 when already at Level 3.");

        if (Status != KycStatus.Level1Verified && Status != KycStatus.Level2Pending)
            throw new DomainException($"Cannot approve Level 2 from status: {Status}");

        if (!HasCompletedLevel2())
            throw new DomainException("Level 2 requirements not met. Need valid identity document verification.");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        if (expiresAt > DateTime.UtcNow.AddYears(2))
            throw new DomainException("Expiration date cannot be more than 2 years in the future.");

        // Ensure at least one identity document is verified and valid
        var validDocuments = _identityDocuments
            .Where(d => d.IsValid &&
                        (d.Type == KycDocumentType.Passport ||
                         d.Type == KycDocumentType.NationalId ||
                         d.Type == KycDocumentType.DriversLicense))
            .ToList();

        if (!validDocuments.Any())
            throw new DomainException("No valid identity documents found for Level 2 verification.");

        var previousStatus = Status;
        Status = KycStatus.Level2Verified;
        VerifiedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        VerificationNotes = notes;
        VerifiedBy = approvedBy;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            previousStatus,
            Status,
            "Level 2 KYC approved",
            approvedBy,
            notes));
    }

    public void ApproveLevel3(string approvedBy, DateTime expiresAt, string? notes = null)
    {
        // Business logic validation
        if (Status == KycStatus.Level3Verified)
            throw new DomainException("KYC Level 3 is already verified.");

        if (Status != KycStatus.Level2Verified && Status != KycStatus.Level3Pending)
            throw new DomainException($"Cannot approve Level 3 from status: {Status}");

        if (!HasCompletedLevel3())
            throw new DomainException("Level 3 requirements not met. Need enhanced verification documents.");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        if (expiresAt > DateTime.UtcNow.AddYears(1))
            throw new DomainException("Level 3 verification cannot exceed 1 year due to enhanced risk.");

        // Enhanced validation for Level 3
        var addressProof = _identityDocuments.FirstOrDefault(d =>
            d.IsValid && d.Type == KycDocumentType.ProofOfAddress);

        var selfieDocument = _identityDocuments.FirstOrDefault(d =>
            d.IsValid && d.Type == KycDocumentType.SelfiePhoto);

        if (addressProof == null)
            throw new DomainException("Proof of address document is required for Level 3 verification.");

        if (selfieDocument == null)
            throw new DomainException("Selfie photo for facial verification is required for Level 3 verification.");

        // Ensure address proof is recent (within last 3 months)
        if (addressProof.IssueDate < DateTime.UtcNow.AddMonths(-3))
            throw new DomainException("Proof of address document must be issued within the last 3 months.");

        // Additional business rules for high-risk clients
        var clientRiskScore = CalculateRiskScore();
        if (clientRiskScore > 75)
            throw new DomainException($"Client risk score ({clientRiskScore}) too high for Level 3 approval. Requires additional review.");

        var previousStatus = Status;
        Status = KycStatus.Level3Verified;
        VerifiedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        VerificationNotes = notes;
        VerifiedBy = approvedBy;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            previousStatus,
            Status,
            "Level 3 KYC approved",
            approvedBy,
            notes));

        // Record enhanced verification details
        RecordEnhancedVerification(addressProof, selfieDocument, approvedBy);
    }

    private void RecordEnhancedVerification(IdentityDocument addressProof, IdentityDocument selfieDocument, string approvedBy)
    {
        // This could be expanded to create enhanced verification records
        // For now, we'll log the approval details
        var enhancedDetails = new
        {
            AddressProofId = addressProof.Id,
            AddressProofType = addressProof.Type,
            AddressProofIssueDate = addressProof.IssueDate,
            SelfieDocumentId = selfieDocument.Id,
            SelfieVerificationDate = selfieDocument.VerificationDetails?.VerifiedAt,
            RiskScore = CalculateRiskScore(),
            ApprovedBy = approvedBy,
            ApprovedAt = DateTime.UtcNow
        };

        // In a real implementation, this would be stored in an enhanced verification entity
        // For now, we can add it to verification notes
        if (string.IsNullOrEmpty(VerificationNotes))
            VerificationNotes = "Enhanced verification completed";
        else
            VerificationNotes += $"\nEnhanced verification completed with risk score: {CalculateRiskScore()}";
    }

    private int CalculateRiskScore()
    {
        // Simplified risk score calculation
        // In a real implementation, this would be more sophisticated
        var score = 0;

        // Document age penalty
        var oldestDocument = _identityDocuments.Where(d => d.IsValid).Min(d => d.IssueDate);
        var documentAgeMonths = (DateTime.UtcNow - oldestDocument).TotalDays / 30;
        if (documentAgeMonths > 12) score += 20;
        else if (documentAgeMonths > 6) score += 10;

        // Document count bonus
        var validDocumentCount = _identityDocuments.Count(d => d.IsValid);
        if (validDocumentCount >= 3) score -= 15;
        else if (validDocumentCount == 2) score -= 5;

        // Document type penalties/bonuses
        var hasInternationalDoc = _identityDocuments.Any(d =>
            d.IsValid && d.Type == KycDocumentType.Passport);
        if (hasInternationalDoc) score -= 10; // Bonus for international document

        var hasMultipleIdTypes = _identityDocuments.Count(d =>
            d.IsValid &&
            (d.Type == KycDocumentType.Passport ||
             d.Type == KycDocumentType.NationalId ||
             d.Type == KycDocumentType.DriversLicense)) >= 2;
        if (hasMultipleIdTypes) score -= 15; // Bonus for multiple ID types

        // Ensure score is between 0-100
        return Math.Max(0, Math.Min(100, score));
    }
    private int GetCurrentKycLevel()
    {
        return Status switch
        {
            KycStatus.Level2Verified => 2,
            KycStatus.Level3Verified => 3,
            _ => 1
        };
    }

    public void SubmitIdentityDocumentForVerification(Guid documentId)
    {
        var document = _identityDocuments.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            throw new DomainException($"Document not found: {documentId}");

        document.MarkAsSubmitted();

        // Update KYC status if needed
        if (Status == KycStatus.Level2Pending || Status == KycStatus.Level1Verified)
        {
            Status = KycStatus.Level2Pending;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyIdentityDocumentByAdmin(Guid documentId, string adminEmail, string? notes = null)
    {
        var document = _identityDocuments.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            throw new DomainException($"Document not found: {documentId}");

        document.VerifyByAdmin(adminEmail, notes);
        TryAdvanceLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyIdentityDocumentByThirdParty(
        Guid documentId,
        string providerName,
        string verificationId,
        string? notes = null)
    {
        var document = _identityDocuments.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            throw new DomainException($"Document not found: {documentId}");

        document.VerifyByThirdParty(providerName, verificationId, notes);
        TryAdvanceLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    // KYC Level Advancement
    public void AdvanceToLevel3()
    {
        if (Status != KycStatus.Level2Verified)
            throw new DomainException($"Cannot advance to Level 3 from status: {Status}");

        Status = KycStatus.Level3Pending;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            KycStatus.Level2Verified,
            Status,
            "Advanced to Level 3 verification",
            "SYSTEM",
            "Waiting for enhanced verification"));
    }

    public void CompleteLevel3Verification(
        string verifiedBy,
        DateTime expiresAt,
        string? notes = null)
    {
        if (Status != KycStatus.Level3Pending)
            throw new DomainException($"Cannot complete Level 3 verification from status: {Status}");

        Status = KycStatus.Level3Verified;
        VerifiedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        VerificationNotes = notes?.Trim();
        VerifiedBy = verifiedBy;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            KycStatus.Level3Pending,
            Status,
            "Level 3 verification completed",
            verifiedBy,
            notes));
    }

    // KYC Verification Workflow
    public void SubmitForReview()
    {
        if (Status != KycStatus.Level2Verified && Status != KycStatus.Level3Verified)
            throw new DomainException($"Cannot submit for review from status: {Status}");

        Status = KycStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            Status,
            KycStatus.UnderReview,
            "Submitted for final review",
            "SYSTEM"));
    }

    public void ApproveKyc(
        string verifiedBy,
        DateTime expiresAt,
        string? notes = null)
    {
        if (Status != KycStatus.UnderReview)
            throw new DomainException($"Cannot approve KYC from status: {Status}");

        var oldStatus = Status;

        // Determine final status based on highest completed level
        if (HasCompletedLevel3())
        {
            Status = KycStatus.Level3Verified;
        }
        else if (HasCompletedLevel2())
        {
            Status = KycStatus.Level2Verified;
        }
        else if (HasCompletedLevel1())
        {
            Status = KycStatus.Level1Verified;
        }
        else
        {
            throw new DomainException("Insufficient verification levels completed");
        }

        VerifiedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        VerificationNotes = notes?.Trim();
        VerifiedBy = verifiedBy;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            oldStatus,
            Status,
            "KYC approved",
            verifiedBy,
            notes));
    }

    public void RejectKyc(string rejectedBy, string reason)
    {
        if (Status != KycStatus.UnderReview)
            throw new DomainException($"Cannot reject KYC from status: {Status}");

        var oldStatus = Status;
        Status = KycStatus.Rejected;
        RejectionReason = reason.Trim();
        RejectedBy = rejectedBy;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            oldStatus,
            Status,
            "KYC rejected",
            rejectedBy,
            reason));
    }

    public void Suspend(string suspendedBy, string reason)
    {
        if (Status != KycStatus.Level1Verified &&
            Status != KycStatus.Level2Verified &&
            Status != KycStatus.Level3Verified)
            throw new DomainException("Only verified KYC profiles can be suspended");

        var oldStatus = Status;
        Status = KycStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            oldStatus,
            Status,
            "KYC suspended",
            suspendedBy,
            reason));
    }

    public void Reinstate(string reinstatedBy, string reason)
    {
        if (Status != KycStatus.Suspended)
            throw new DomainException("Only suspended KYC profiles can be reinstated");

        var oldStatus = Status;

        // Restore to previous verified status
        if (HasCompletedLevel3())
        {
            Status = KycStatus.Level3Verified;
        }
        else if (HasCompletedLevel2())
        {
            Status = KycStatus.Level2Verified;
        }
        else if (HasCompletedLevel1())
        {
            Status = KycStatus.Level1Verified;
        }
        else
        {
            Status = KycStatus.Level1Verified;
        }

        UpdatedAt = DateTime.UtcNow;

        _verificationHistory.Add(KycVerificationHistory.Create(
            Id,
            oldStatus,
            Status,
            "KYC reinstated",
            reinstatedBy,
            reason));
    }

    public void CheckExpiration()
    {
        if (IsVerified() && ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
        {
            var oldStatus = Status;
            Status = KycStatus.Expired;
            UpdatedAt = DateTime.UtcNow;

            _verificationHistory.Add(KycVerificationHistory.Create(
                Id,
                oldStatus,
                Status,
                "KYC expired",
                "SYSTEM",
                $"Expired on {ExpiresAt.Value:yyyy-MM-dd}"));
        }

        // Check document expirations
        foreach (var document in _identityDocuments)
        {
            document.CheckExpiration();
        }
    }

    // Business Rules and Permissions
    public bool CanMakeDeposit()
    {
        return IsLevel1Verified() || IsLevel2Verified() || IsLevel3Verified();
    }

    public bool CanMakeStandardPurchase()
    {
        return IsLevel1Verified() || IsLevel2Verified() || IsLevel3Verified();
    }

    public bool CanMakeLargePurchase()
    {
        return IsLevel2Verified() || IsLevel3Verified();
    }

    public bool CanMakeInternationalTransfer()
    {
        return IsLevel2Verified() || IsLevel3Verified();
    }

    public bool CanAccessAdvancedFeatures()
    {
        return IsLevel3Verified();
    }

    // Status check methods
    public bool IsLevel1Verified() => Status == KycStatus.Level1Verified;
    public bool IsLevel2Verified() => Status == KycStatus.Level2Verified;
    public bool IsLevel3Verified() => Status == KycStatus.Level3Verified;
    public bool IsVerified() => IsLevel1Verified() || IsLevel2Verified() || IsLevel3Verified();
    public bool IsPendingVerification() =>
        Status == KycStatus.Level1Pending ||
        Status == KycStatus.Level2Pending ||
        Status == KycStatus.Level3Pending;
    public bool IsUnderReview() => Status == KycStatus.UnderReview;
    public bool IsRejected() => Status == KycStatus.Rejected;
    public bool IsExpired() => Status == KycStatus.Expired;
    public bool IsSuspended() => Status == KycStatus.Suspended;

    // Level completion checks
    public bool HasCompletedLevel1()
    {
        return EmailVerification?.IsVerified == true &&
               PhoneVerification?.IsVerified == true;
    }

    public bool HasCompletedLevel2()
    {
        return HasCompletedLevel1() &&
               _identityDocuments.Any(d => d.IsValid &&
                   (d.Type == KycDocumentType.Passport ||
                    d.Type == KycDocumentType.NationalId ||
                    d.Type == KycDocumentType.DriversLicense));
    }

    public bool HasCompletedLevel3()
    {
        // Level 3 requires additional verification (biometric, address proof, etc.)
        return HasCompletedLevel2() &&
               _identityDocuments.Any(d => d.IsValid && d.Type == KycDocumentType.ProofOfAddress) &&
               _identityDocuments.Any(d => d.IsValid && d.Type == KycDocumentType.SelfiePhoto);
    }

    // Private helper methods
    private void TryAdvanceLevel()
    {
        // Check if we can advance to Level 1
        if (Status == KycStatus.NotStarted || Status == KycStatus.Level1Pending)
        {
            if (HasCompletedLevel1())
            {
                Status = KycStatus.Level1Verified;
                _verificationHistory.Add(KycVerificationHistory.Create(
                    Id,
                    Status,
                    KycStatus.Level1Verified,
                    "Level 1 verification completed",
                    "SYSTEM"));
            }
            else if (EmailVerification != null || PhoneVerification != null)
            {
                Status = KycStatus.Level1Pending;
            }
        }

        // Check if we can advance to Level 2
        if (Status == KycStatus.Level1Verified || Status == KycStatus.Level2Pending)
        {
            if (HasCompletedLevel2())
            {
                Status = KycStatus.Level2Verified;
                _verificationHistory.Add(KycVerificationHistory.Create(
                    Id,
                    Status,
                    KycStatus.Level2Verified,
                    "Level 2 verification completed",
                    "SYSTEM"));
            }
            else if (_identityDocuments.Any())
            {
                Status = KycStatus.Level2Pending;
            }
        }
    }
}

// KYC Verification History Entity
public class KycVerificationHistory : Entity<Guid>
{
    public Guid KycProfileId { get; private init; }
    public KycStatus OldStatus { get; private init; }
    public KycStatus NewStatus { get; private init; }
    public string Action { get; private init; }
    public string PerformedBy { get; private init; }
    public string? Notes { get; private init; }
    public DateTime PerformedAt { get; private init; }

    // Private constructor for EF Core
    private KycVerificationHistory()
    {
        Action = string.Empty;
        PerformedBy = string.Empty;
    }

    public static KycVerificationHistory Create(
        Guid kycProfileId,
        KycStatus oldStatus,
        KycStatus newStatus,
        string action,
        string performedBy,
        string? notes = null)
    {
        DomainGuards.AgainstDefault(kycProfileId);
        DomainGuards.AgainstNullOrWhiteSpace(action);
        DomainGuards.AgainstNullOrWhiteSpace(performedBy);

        return new KycVerificationHistory
        {
            Id = Guid.NewGuid(),
            KycProfileId = kycProfileId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Action = action.Trim(),
            PerformedBy = performedBy.Trim(),
            Notes = notes?.Trim(),
            PerformedAt = DateTime.UtcNow
        };
    }
}