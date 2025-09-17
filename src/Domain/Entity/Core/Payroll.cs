using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Core;

public class Payroll : Entity<string>
{
    public Guid? PublicId { get; private set; }
    public string Description { get; private set; } = null!;
    public string PayMonth { get; private set; } = null!;
    public string PayPeriod { get; private set; } = null!;
    public string TransYear { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public double DaysInMonth { get; private set; }
    public double PayrollDays { get; private set; }
    public string Remark { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    protected Payroll()
    {
    }

    public static Payroll Create(
        string id,
        string description,
        string payMonth,
        string payPeriod,
        string transYear,
        DateTime startDate,
        DateTime endDate,
        double daysInMonth,
        double payrollDays,
        string remark,
        string status,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(payMonth);
        DomainGuards.AgainstNullOrWhiteSpace(payPeriod);
        DomainGuards.AgainstNullOrWhiteSpace(transYear);
        DomainGuards.AgainstNullOrWhiteSpace(remark);
        DomainGuards.AgainstNullOrWhiteSpace(status);

        if (daysInMonth < 0) throw new ArgumentOutOfRangeException(nameof(daysInMonth), "Days in month cannot be negative.");
        if (payrollDays < 0) throw new ArgumentOutOfRangeException(nameof(payrollDays), "Payroll days cannot be negative.");
        if (endDate < startDate) throw new ArgumentException("End date cannot be before start date.");

        return new Payroll
        {
            Id = id,
            Description = description,
            PayMonth = payMonth,
            PayPeriod = payPeriod,
            TransYear = transYear,
            StartDate = startDate,
            EndDate = endDate,
            DaysInMonth = daysInMonth,
            PayrollDays = payrollDays,
            Remark = remark,
            Status = status,
            CreatedOn = createdOn ?? DateTime.UtcNow
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
