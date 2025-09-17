namespace Agrovet.Domain.ValueObjects;

public class Child
{
    public string FullName { get; }
    public DateTime DateOfBirth { get; }

    public Child(string fullName, DateTime dateOfBirth)
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
    }
}