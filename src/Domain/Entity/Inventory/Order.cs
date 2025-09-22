using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class Order : Entity<string>
{
    
    public string OrderType { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
    public string Status { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Supplier { get; private set; } = null!;
    public DateTime TransDate { get; private set; }
    public Guid? PublicId { get; private set; }
    public DateTime CreatedOn { get; private set; }

    private readonly List<OrderDetail> _orderDetails = [];
    public IReadOnlyCollection<OrderDetail> OrderDetails => _orderDetails.AsReadOnly();

    protected Order() { }

    public static Order Create(string orderType, DateTime orderDate, string status, string description, string supplier,
        DateTime transDate, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(orderType);
        DomainGuards.AgainstNullOrWhiteSpace(status);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(supplier);

        return new Order
        {
            OrderType = orderType,
            OrderDate = orderDate,
            Status = status,
            Description = description,
            Supplier = supplier,
            TransDate = transDate,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void AddOrderDetail(OrderDetail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail));

        // Enforce consistency: the OrderDetail.OrderId must match this Order.Id
        if (string.IsNullOrWhiteSpace(detail.OrderId) || detail.OrderId != Id)
        {
            typeof(OrderDetail)
                .GetProperty("OrderId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(detail, Id);
        }

        _orderDetails.Add(detail);
    }

    public void RemoveOrderDetail(OrderDetail detail)
    {
        if (detail == null) throw new ArgumentNullException(nameof(detail));
        _orderDetails.Remove(detail);
    }
}
