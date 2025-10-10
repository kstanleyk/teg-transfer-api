using Transfer.Domain.Exceptions;

namespace Transfer.Domain.ValueObjects;

public class EmploymentInfo : ValueObject
{
    public DateTime? EmploymentDate { get; }
    public DateTime? RetirementDate { get; }
    public string EmploymentPlace { get; }
    public string EmploymentAuthority { get; }
    public string EmploymentStatusId { get; }

    private EmploymentInfo()
    {
        // For EF Core
    }

    private EmploymentInfo(DateTime employmentDate, DateTime? retirementDate, string employmentPlace,
        string employmentAuthority, string employmentStatusId)
    {
        EmploymentDate = employmentDate;
        RetirementDate = retirementDate;
        EmploymentPlace = employmentPlace;
        EmploymentAuthority = employmentAuthority;
        EmploymentStatusId = employmentStatusId;
    }

    public static EmploymentInfo Create(DateTime employmentDate, DateTime? retirementDate, string employmentPlace,
        string employmentAuthority, string employmentStatusId)
    {
        if (employmentDate == default)
            throw new ArgumentException("Employment date is required.", nameof(employmentDate));
        if (string.IsNullOrWhiteSpace(employmentPlace))
            throw new ArgumentException("Employment place is required.", nameof(employmentPlace));
        if (string.IsNullOrWhiteSpace(employmentAuthority))
            throw new ArgumentException("Employment authority is required.", nameof(employmentAuthority));
        if (string.IsNullOrWhiteSpace(employmentStatusId))
            throw new ArgumentException("Employment status ID is required.", nameof(employmentStatusId));
        if (retirementDate.HasValue && retirementDate.Value < employmentDate)
            throw new DomainException("Retirement date must be after employment date.");
        return new EmploymentInfo(employmentDate, retirementDate, employmentPlace, employmentAuthority,
            employmentStatusId);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EmploymentDate;
        yield return RetirementDate;
        yield return EmploymentPlace;
        yield return EmploymentAuthority;
        yield return EmploymentStatusId;
    }
}
