using Agrovet.Domain.Models.Core.Employee;

namespace Agrovet.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public PhoneNumber Phone1 { get; private set; }
    public PhoneNumber? Phone2 { get; private set; }
    public EmailAddress WorkEmail { get; private set; }
    public EmailAddress? PersonalEmail { get; private set; }
    public string EmergencyContactName { get; private set; }
    public PhoneNumber EmergencyContactNumber { get; private set; }

    private ContactInfo()
    {
        // For EF Core
    }

    private ContactInfo(PhoneNumber phone1, PhoneNumber? phone2, EmailAddress workEmail, EmailAddress personalEmail,
        string emergencyContactName, PhoneNumber emergencyContactNumber)
    {
        Phone1 = phone1;
        Phone2 = phone2;
        WorkEmail = workEmail;
        PersonalEmail = personalEmail;
        EmergencyContactName = emergencyContactName;
        EmergencyContactNumber = emergencyContactNumber;
    }

    public static ContactInfo Create(PhoneNumberDto phone1, PhoneNumberDto? phone2, string workEmail, string? personalEmail,
        string emergencyContactName, PhoneNumberDto emergencyContactNumber)
    {
        if (string.IsNullOrEmpty(phone1.Number)) throw new ArgumentNullException(nameof(phone1.Number), "Phone1 cannot be null.");

        if (string.IsNullOrEmpty(workEmail)) throw new ArgumentNullException(nameof(workEmail), "workEmail cannot be null.");
        if (string.IsNullOrEmpty(emergencyContactName)) throw new ArgumentNullException(nameof(emergencyContactName), "emergencyContactName cannot be null.");
        if (string.IsNullOrEmpty(emergencyContactNumber.Number)) throw new ArgumentNullException(nameof(emergencyContactNumber.Number), "personalEmail cannot be null.");

        var telephone1 = PhoneNumber.Of(phone1.Number, phone1.CountryCode);
        var telephone2 = phone2 != null ? PhoneNumber.Of(phone2.Number, phone2.CountryCode) : PhoneNumber.Of("");
        var workEmailAddress = EmailAddress.Of(workEmail);
        var personalEmailAddress = personalEmail != null ? EmailAddress.Of(personalEmail) : EmailAddress.Of(string.Empty);
        var emergencyContactTelephone = PhoneNumber.Of(emergencyContactNumber.Number, emergencyContactNumber.CountryCode);

        return new ContactInfo(telephone1, telephone2, workEmailAddress, personalEmailAddress, emergencyContactName, emergencyContactTelephone);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Phone1;
        yield return Phone2!;
        yield return WorkEmail;
        yield return PersonalEmail!;
        yield return EmergencyContactName;
        yield return EmergencyContactNumber;
    }
}