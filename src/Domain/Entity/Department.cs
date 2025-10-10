using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity;

public class Department : Entity<string>
{
    public string Name { get; private set; } = null!;
    public string FacultyId { get; private set; } = null!;

    protected Department()
    {

    }

    public static Department Create(string name, string facultyId, DateTime createdOn)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);
        DomainGuards.AgainstNullOrWhiteSpace(facultyId);

        return new Department
        {
            Name = name,
            FacultyId = facultyId,
            CreatedOn = createdOn
        };
    }
}