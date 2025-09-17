namespace Agrovet.Domain.Entity.Core;

public class PayrollAverageWeight
{ 
    public string PayrollId { get; private set; } = null!;
    public string EstateId { get; private set; } = null!;
    public string BlockId { get; private set; } = null!;
    public double AverageFruitBunchWeight { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected PayrollAverageWeight()
    {
    }

    public static PayrollAverageWeight Create(
        string payrollId,
        string estateId,
        string blockId,
        double averageFruitBunchWeight,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(payrollId);
        DomainGuards.AgainstNullOrWhiteSpace(estateId);
        DomainGuards.AgainstNullOrWhiteSpace(blockId);

        return new PayrollAverageWeight
        {
            PayrollId = payrollId,
            EstateId = estateId,
            BlockId = blockId,
            AverageFruitBunchWeight = averageFruitBunchWeight,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }
}
