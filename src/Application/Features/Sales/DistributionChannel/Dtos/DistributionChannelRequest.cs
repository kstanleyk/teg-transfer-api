namespace Agrovet.Application.Features.Sales.DistributionChannel.Dtos;

public abstract class BaseDistributionChannelRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get;  set; }
    public bool IsActive { get; set; }
}

public class CreateDistributionChannelRequest : BaseDistributionChannelRequest
{
    
}

public class EditDistributionChannelRequest : BaseDistributionChannelRequest
{
    public string Id { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

public class DistributionChannelValidationCodes
{
    public IEnumerable<string> ValidIds { get; set; } = [];
}