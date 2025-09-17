using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity;

public class Department : Entity<string>
{
    public string Name { get; private set; } = null!;
    public string FacultyId { get; private set; } = null!;
    public Guid? PublicId { get; private set; }
    public DateTime CreatedOn { get; private set; }

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

    public void SetId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }
}