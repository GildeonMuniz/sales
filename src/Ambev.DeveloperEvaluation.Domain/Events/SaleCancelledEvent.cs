using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCancelledEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public DateTime CancelledAt { get; }

    public SaleCancelledEvent(Guid saleId, string saleNumber, DateTime cancelledAt)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        CancelledAt = cancelledAt;
    }
}
