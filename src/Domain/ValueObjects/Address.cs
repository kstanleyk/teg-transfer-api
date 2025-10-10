using Transfer.Domain.Entity;
using Transfer.Domain.Exceptions;

namespace Transfer.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }

    // Additional fields for flexible addressing
    public string Quarter { get; private set; }      // Common in Cameroon
    public string Landmark { get; private set; }     // Common when addresses are informal

    private Address() { } // EF Core constructor

    private Address(string street, string city, string state, string country, string zipCode, string quarter, string landmark)
    {
        // Validate based on country
        ValidateAddress(street, city, state, country, zipCode, quarter, landmark);

        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
        Quarter = quarter;
        Landmark = landmark;
    }

    public static Address Create(string street, string city, string state, string country, string zipCode, string quarter = "", string landmark = "")
    {
        return new Address(street, city, state, country, zipCode, quarter, landmark);
    }

    public static Address CreateUsAddress(string street, string city, string state, string zipCode)
    {
        return new Address(street, city, state, "US", zipCode, "", "");
    }

    public static Address CreateCameroonAddress(string city, string quarter = "", string landmark = "", string region = "")
    {
        // For Cameroon, many fields can be empty
        return new Address("", city, region, "CM", "", quarter, landmark);
    }

    private void ValidateAddress(string street, string city, string state, string country, string zipCode, string quarter, string landmark)
    {
        DomainGuards.AgainstNullOrWhiteSpace(country);
        DomainGuards.AgainstNullOrWhiteSpace(city);

        var countryUpper = country.ToUpperInvariant();

        if (countryUpper == "US" || countryUpper == "UNITED STATES")
        {
            // Strict validation for US addresses
            DomainGuards.AgainstNullOrWhiteSpace(street, "Street is required for US addresses");
            DomainGuards.AgainstNullOrWhiteSpace(state, "State is required for US addresses");
            DomainGuards.AgainstNullOrWhiteSpace(zipCode, "ZIP code is required for US addresses");

            if (!System.Text.RegularExpressions.Regex.IsMatch(zipCode, @"^\d{5}(-\d{4})?$"))
                throw new DomainException("Invalid US ZIP code format");

            if (state.Length != 2)
                throw new DomainException("US state must be a 2-letter code");
        }
        else if (countryUpper == "CM" || countryUpper == "CAMEROON")
        {
            // Flexible validation for Cameroon - only require city
            // Other fields are optional but at least one location identifier should be provided
            if (string.IsNullOrWhiteSpace(street) &&
                string.IsNullOrWhiteSpace(quarter) &&
                string.IsNullOrWhiteSpace(landmark))
            {
                throw new DomainException("Cameroon addresses require at least one location identifier (street, quarter, or landmark)");
            }
        }
        else
        {
            // Default validation for other countries
            DomainGuards.AgainstNullOrWhiteSpace(street, "Street is required");
            DomainGuards.AgainstNullOrWhiteSpace(state, "State/Province is required");
        }
    }

    public string GetFormattedAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Street)) parts.Add(Street);
        if (!string.IsNullOrWhiteSpace(Quarter)) parts.Add($"Quarter: {Quarter}");
        if (!string.IsNullOrWhiteSpace(Landmark)) parts.Add($"Near: {Landmark}");
        if (!string.IsNullOrWhiteSpace(City)) parts.Add(City);
        if (!string.IsNullOrWhiteSpace(State)) parts.Add(State);
        if (!string.IsNullOrWhiteSpace(ZipCode)) parts.Add(ZipCode);
        if (!string.IsNullOrWhiteSpace(Country)) parts.Add(Country);

        return string.Join(", ", parts);
    }

    public bool IsUSAddress() => Country.ToUpperInvariant() == "US" || Country.ToUpperInvariant() == "UNITED STATES";
    public bool IsCameroonAddress() => Country.ToUpperInvariant() == "CM" || Country.ToUpperInvariant() == "CAMEROON";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street ?? "";
        yield return City ?? "";
        yield return State ?? "";
        yield return Country ?? "";
        yield return ZipCode ?? "";
        yield return Quarter ?? "";
        yield return Landmark ?? "";
    }
}