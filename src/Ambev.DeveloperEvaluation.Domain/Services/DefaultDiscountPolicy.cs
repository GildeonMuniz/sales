namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Default quantity-based discount policy:
/// - Below 4 items: no discount
/// - 4–9 items: 10% discount
/// - 10–20 items: 20% discount
/// - Above 20 items: not allowed
/// </summary>
public class DefaultDiscountPolicy : IDiscountPolicy
{
    private const int MaxQuantity = 20;
    private const int TierTwoMin = 10;
    private const int TierOneMin = 4;
    private const decimal TierTwoRate = 0.20m;
    private const decimal TierOneRate = 0.10m;

    public decimal GetDiscount(int quantity)
    {
        if (quantity >= TierTwoMin)
            return TierTwoRate;

        if (quantity >= TierOneMin)
            return TierOneRate;

        return 0m;
    }

    public void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (quantity > MaxQuantity)
            throw new DomainException($"Cannot sell more than {MaxQuantity} identical items.");
    }
}
