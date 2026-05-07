namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Defines the contract for quantity-based discount calculation.
/// Implementations can be swapped without changing domain entities.
/// </summary>
public interface IDiscountPolicy
{
    /// <summary>
    /// Returns the discount rate (0–1) applicable for the given quantity.
    /// </summary>
    decimal GetDiscount(int quantity);

    /// <summary>
    /// Throws <see cref="DomainException"/> if the quantity violates business constraints.
    /// </summary>
    void ValidateQuantity(int quantity);
}
