using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCreatedEvent : IDomainEvent
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public DateTime SaleDate { get; }
    public Guid CustomerId { get; }
    public string CustomerName { get; }
    public decimal TotalAmount { get; }

    public SaleCreatedEvent(Guid saleId, string saleNumber, DateTime saleDate, Guid customerId, string customerName, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        TotalAmount = totalAmount;
    }
}
