using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Aggregate root representing a sales transaction.
/// External references (Customer, Branch, Product) use the External Identities pattern:
/// only the ID and a denormalized name snapshot are stored.
/// </summary>
public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;

    public DateTime SaleDate { get; private set; }

    /// <summary>External identity — Customer ID from the Customers domain.</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>Denormalized snapshot of the customer name at the time of sale.</summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>External identity — Branch ID from the Branches domain.</summary>
    public Guid BranchId { get; private set; }

    /// <summary>Denormalized snapshot of the branch name at the time of sale.</summary>
    public string BranchName { get; private set; } = string.Empty;

    public decimal TotalAmount { get; private set; }

    public bool IsCancelled { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected Sale() { }

    public Sale(string saleNumber, DateTime saleDate, Guid customerId, string customerName, Guid branchId, string branchName)
    {
        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string saleNumber, DateTime saleDate, Guid customerId, string customerName, Guid branchId, string branchName)
    {
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds or merges an item into the sale, applying the discount policy.
    /// If the same product already exists (not cancelled), quantities are merged.
    /// </summary>
    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice, IDiscountPolicy discountPolicy)
    {
        if (IsCancelled)
            throw new DomainException("Cannot add items to a cancelled sale.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existing is not null)
        {
            existing.Update(existing.Quantity + quantity, unitPrice, discountPolicy);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var item = new SaleItem(Id, productId, productName, quantity, unitPrice, discountPolicy);
        _items.Add(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new SaleCancelledEvent(Id, SaleNumber, UpdatedAt.Value));
    }

    public void CancelItem(Guid itemId)
    {
        if (IsCancelled)
            throw new DomainException("Cannot cancel items of an already cancelled sale.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} not found in this sale.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new ItemCancelledEvent(Id, item.Id, item.ProductId, item.ProductName, UpdatedAt.Value));
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }
}
