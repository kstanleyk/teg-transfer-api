namespace Transfer.Application.Features.Sales.DistributionChannel.Dtos;

public class DistributionChannelResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class DistributionChannelCreatedResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class DistributionChannelUpdatedResponse
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}