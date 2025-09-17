namespace Agrovet.Domain.ValueObjects;

public sealed class PersonalInfo: ValueObject
{
    public string FullName { get; }
    public DateTime DateOfBirth { get; }
    public string PlaceOfBirth { get; }
    public string BirthCertificateNumber { get; }
    public string GenderId { get; }
    public string FatherName { get; }
    public string MotherName { get; }

    private PersonalInfo()
    {
        // For EF Core
    }

    private PersonalInfo(string fullName, DateTime dateOfBirth, string placeOfBirth, string birthCertificateNumber,
        string genderId, string fatherName, string motherName)
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        PlaceOfBirth = placeOfBirth;
        BirthCertificateNumber = birthCertificateNumber;
        GenderId = genderId;
        FatherName = fatherName;
        MotherName = motherName;
    }

    public static PersonalInfo Create(string fullName, DateTime dateOfBirth, string placeOfBirth,
        string birthCertificateNumber, string genderId, string fatherName, string motherName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentNullException(nameof(fullName), "Employee Name is required");

        if (dateOfBirth == default)
            throw new ArgumentException("Date of birth is required", nameof(dateOfBirth));

        if (string.IsNullOrWhiteSpace(placeOfBirth))
            throw new ArgumentNullException(nameof(placeOfBirth), "Place of birth is required");

        if (string.IsNullOrWhiteSpace(birthCertificateNumber))
            throw new ArgumentNullException(nameof(birthCertificateNumber), "Birth certificate number is required");

        if (string.IsNullOrWhiteSpace(genderId))
            throw new ArgumentNullException(nameof(genderId), "Gender is required");

        if (string.IsNullOrWhiteSpace(fatherName))
            throw new ArgumentNullException(nameof(fatherName), "Father's name is required");

        if (string.IsNullOrWhiteSpace(motherName))
            throw new ArgumentNullException(nameof(motherName), "Mother's name is required");

        return new PersonalInfo(fullName, dateOfBirth, placeOfBirth, birthCertificateNumber, genderId, fatherName,
            motherName);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return DateOfBirth;
        yield return PlaceOfBirth;
        yield return BirthCertificateNumber;
        yield return GenderId;
        yield return FatherName;
        yield return MotherName;
    }
}
