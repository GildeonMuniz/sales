using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleModifiedEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public decimal TotalAmount { get; }
    public DateTime ModifiedAt { get; }

    public SaleModifiedEvent(Guid saleId, string saleNumber, decimal totalAmount, DateTime modifiedAt)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        TotalAmount = totalAmount;
        ModifiedAt = modifiedAt;
    }
}
