using Agrovet.Domain.Exceptions;

namespace Agrovet.Domain.ValueObjects;

public class IdentityInfo : ValueObject
{
    public string Number { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public string PlaceOfIssue { get; private set; }

    private IdentityInfo()
    {
        // For EF Core
    }

    private IdentityInfo(string number, DateTime? issueDate, string placeOfIssue)
    {
        Number = number;
        IssueDate = issueDate;
        PlaceOfIssue = placeOfIssue;
    }

    public static IdentityInfo Create(string number, DateTime? issueDate, string placeOfIssue)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentNullException(nameof(number), "ID number is required");

        if (string.IsNullOrWhiteSpace(placeOfIssue))
            throw new ArgumentNullException(nameof(placeOfIssue), "Place of issue is required");

        if (issueDate == null)
            throw new ArgumentNullException(nameof(issueDate), "Place of issue is required");

        if (issueDate > DateTime.Today)
            throw new DomainException("Issue date cannot be in the future");

        return new IdentityInfo(number, issueDate, placeOfIssue);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
        yield return IssueDate.GetValueOrDefault();
        yield return PlaceOfIssue;
    }
}
