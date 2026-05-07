using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelledEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public Guid ItemId { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }
    public DateTime CancelledAt { get; }

    public ItemCancelledEvent(Guid saleId, Guid itemId, Guid productId, string productName, DateTime cancelledAt)
    {
        SaleId = saleId;
        ItemId = itemId;
        ProductId = productId;
        ProductName = productName;
        CancelledAt = cancelledAt;
    }
}
