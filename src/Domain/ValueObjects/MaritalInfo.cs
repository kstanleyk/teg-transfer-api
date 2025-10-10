using Transfer.Domain.Exceptions;

namespace Transfer.Domain.ValueObjects;

public class MaritalInfo : ValueObject
{
    public string MaritalStatusId { get; }
    public string SpouseName { get; }
    public string MarriageCertificateNumber { get; }
    public DateTime? MarriageDate { get; }

    private MaritalInfo()
    {
        // For EF Core
    }

    private MaritalInfo(string maritalStatusId, string spouseName, string marriageCertificateNumber,
        DateTime? marriageDate)
    {
        MaritalStatusId = maritalStatusId;
        SpouseName = spouseName;
        MarriageCertificateNumber = marriageCertificateNumber;
        MarriageDate = marriageDate;
    }

    public static MaritalInfo Create(string maritalStatusId, string spouseName, string marriageCertificateNumber,
        DateTime? marriageDate)
    {
        if (string.IsNullOrWhiteSpace(maritalStatusId))
            throw new DomainException("Marital status is required");

        if (maritalStatusId == "02" && string.IsNullOrWhiteSpace(spouseName))
            throw new DomainException("Spouse name is required for married employees");

        if (maritalStatusId == "02" && string.IsNullOrWhiteSpace(marriageCertificateNumber))
            throw new DomainException("Marriage certificateNumber is required for married employees");

        if (maritalStatusId == "02" && !marriageDate.HasValue)
            throw new DomainException("Marriage date is required for married employees");

        return new MaritalInfo(maritalStatusId,spouseName, marriageCertificateNumber, marriageDate);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaritalStatusId;
        yield return SpouseName;
        yield return MarriageCertificateNumber;
        yield return (object?)MarriageDate ?? null!;
    }
}