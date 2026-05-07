using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Discount rate between 0 and 1, calculated by the active <see cref="IDiscountPolicy"/>.
    /// </summary>
    public decimal Discount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public bool IsCancelled { get; private set; }

    protected SaleItem() { }

    public SaleItem(Guid saleId, Guid productId, string productName, int quantity, decimal unitPrice, IDiscountPolicy discountPolicy)
    {
        Id = Guid.NewGuid();
        SaleId = saleId;
        ProductId = productId;
        ProductName = productName;
        Apply(quantity, unitPrice, discountPolicy);
    }

    public void Update(int quantity, decimal unitPrice, IDiscountPolicy discountPolicy)
    {
        if (IsCancelled)
            throw new DomainException("Cannot update a cancelled item.");

        Apply(quantity, unitPrice, discountPolicy);
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Item is already cancelled.");

        IsCancelled = true;
    }

    private void Apply(int quantity, decimal unitPrice, IDiscountPolicy discountPolicy)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        discountPolicy.ValidateQuantity(quantity);

        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discountPolicy.GetDiscount(quantity);
        TotalAmount = unitPrice * quantity * (1 - Discount);
    }
}
