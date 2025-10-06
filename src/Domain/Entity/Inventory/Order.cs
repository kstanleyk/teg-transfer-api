using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class Order : Aggregate<string>
{
    public string OrderType { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
    public string Status { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Supplier { get; private set; } = null!;
    public DateTime TransDate { get; private init; }

    private readonly List<OrderDetail> _orderDetails = [];
    private readonly List<ProductMovement> _itemMovements = [];

    public IReadOnlyCollection<OrderDetail> OrderDetails => _orderDetails.AsReadOnly();
    public IReadOnlyCollection<ProductMovement> ItemMovements => _itemMovements.AsReadOnly();

    protected Order() { }

    public static Order Create(string orderType, DateTime orderDate, string status, string description, string supplier,
        DateTime transDate, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(orderType);
        DomainGuards.AgainstNullOrWhiteSpace(status);
        DomainGuards.AgainstNullOrWhiteSpace(description);
        DomainGuards.AgainstNullOrWhiteSpace(supplier);

        var createdDate = createdOn ?? DateTime.UtcNow;

        return new Order
        {
            OrderType = orderType,
            OrderDate = orderDate.ToUtcDate(),
            Status = status,
            Description = description,
            Supplier = supplier,
            TransDate = transDate.ToUtcDate(),
            CreatedOn = createdDate.ToUtcDate()
        };
    }

    public void Update(Order order)
    {
        DomainGuards.AgainstNullOrWhiteSpace(order.OrderType);
        DomainGuards.AgainstNullOrWhiteSpace(order.Status);
        DomainGuards.AgainstNullOrWhiteSpace(order.Description);
        DomainGuards.AgainstNullOrWhiteSpace(order.Supplier);

        OrderType = order.OrderType;
        Status = order.Status;
        Description = order.Description;
        Supplier = order.Supplier;
    }

    public bool HasChanges(Order? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return OrderType != other.OrderType ||
               Status != other.Status ||
               Description != other.Description ||
               Supplier != other.Supplier;
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
        AttachOrderDetails();
    }

    public void AddOrderDetail(OrderDetail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail));

        // Enforce consistency: the OrderDetail.OrderId must match this Order.Id
        //if (string.IsNullOrWhiteSpace(detail.OrderId) || detail.OrderId != Id)
        //{
        //    typeof(OrderDetail)
        //        .GetProperty("OrderId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
        //        .SetValue(detail, Id);
        //}

        var lineNum = _orderDetails.Count + 1;
        detail.SetLineNum(lineNum.ToString().PadLeft(4, '0'));

        _orderDetails.Add(detail);
    }

    public void AttachOrderDetails()
    {
        var i = 0;

        foreach (var orderDetail in _orderDetails)
        {
            var lineNum = i + 1;
            orderDetail.SetLineNum(lineNum.ToString().PadLeft(4, '0'));
            orderDetail.SetOrderId(Id);
            orderDetail.SetStatus(Status);
            i++;
        }
    }

    public void UpdateItemMovements(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);

        _itemMovements.Clear();

        foreach (var orderDetail in _orderDetails)
        {
            var movement = ProductMovement.Create(orderDetail.LineNum,
                Description,
                orderDetail.Item,
                TransDate,
                DateTime.Now.ToLongTimeString(),
                ProductMovement.MovementDirection.Inward,
                orderDetail.Qtty,
                orderDetail.Id,
                orderDetail.LineNum);
            movement.SetId(id);
            _itemMovements.Add(movement);
        }
    }

    public void RemoveOrderDetail(OrderDetail detail)
    {
        if (detail == null) throw new ArgumentNullException(nameof(detail));
        _orderDetails.Remove(detail);
    }

    public void MarkAsPendingSubmission()
    {
        Status = OrderStatus.PendingSubmission;
        UpdateOrderDetailStatus();
    }

    private void UpdateOrderDetailStatus()
    {
        foreach (var orderDetail in _orderDetails)
        {
            orderDetail.SetStatus(Status);
        }
    }

    public void MarkAsSubmitted()
    {
        if (Status != OrderStatus.PendingSubmission)
            throw new InvalidOperationException("Order must be in 'Pending Submission' state before submission.");

        if(_orderDetails.Count == 0)
            throw new InvalidOperationException("You cannot submit an order without any items.");

        Status = OrderStatus.Submitted;
        UpdateOrderDetailStatus();
    }

    public void MarkAsValidated()
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException("Order must be 'Submitted' before it can be validated.");

        if (_orderDetails.Count == 0)
            throw new InvalidOperationException("You cannot validate an order without any items.");

        Status = OrderStatus.Validated;
        UpdateOrderDetailStatus();
    }

    public void MarkAsReceived()
    {
        if (Status != OrderStatus.Validated)
            throw new InvalidOperationException("Order must be 'Validated' before it can be received.");

        if (_orderDetails.Count == 0)
            throw new InvalidOperationException("You cannot receive an order without any items.");

        Status = OrderStatus.Received;
        UpdateOrderDetailStatus();
    }

    public void MarkAsCancelled()
    {
        if (Status != OrderStatus.Received)
            throw new InvalidOperationException("You can not cancel an order that has already been received.");

        Status = OrderStatus.Cancelled;
        UpdateOrderDetailStatus();
    }

    internal static class OrderStatus
    {
        public const string PendingSubmission = "50";
        public const string Submitted = "51";
        public const string Validated = "52";
        public const string Received = "53";
        public const string Cancelled = "54";
    }
}