namespace Transfer.Domain.ValueObjects;

public class NationalityInfo : ValueObject
{
    public string Village { get; }
    public string SubDivisionId { get; }
    public string CountryId { get; }

    private NationalityInfo()
    {
        // For EF Core
    }

    private NationalityInfo(
        string village,
        string subDivisionId,
        string countryId)
    {
        Village = village;
        SubDivisionId = subDivisionId;
        CountryId = countryId;
    }

    public static NationalityInfo Create(string village, string subDivisionId, string countryId)
    {
        if (string.IsNullOrEmpty(village))
            throw new ArgumentException("Village is required.", nameof(village));

        if (countryId == "001" && string.IsNullOrWhiteSpace(subDivisionId))
            throw new ArgumentException("SubDivisionId is required when CountryId is '001'.", nameof(subDivisionId));

        if (string.IsNullOrEmpty(countryId))
            throw new ArgumentException("countryId is required.", nameof(village));

        return new NationalityInfo(village, subDivisionId, countryId);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Village;
        yield return SubDivisionId;
        yield return CountryId;
    }
}
