using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

/// <summary>
/// Contains unit tests for the <see cref="DefaultDiscountPolicy"/> class.
/// Tests cover all discount tiers and quantity constraints.
/// </summary>
public class DefaultDiscountPolicyTests
{
    private readonly DefaultDiscountPolicy _policy = new();

    [Theory(DisplayName = "Quantities below 4 should have no discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Given_QuantityBelow4_When_GetDiscount_Then_ReturnsZero(int quantity)
    {
        // Act
        var discount = _policy.GetDiscount(quantity);

        // Assert
        discount.Should().Be(0m);
    }

    [Theory(DisplayName = "Quantities from 4 to 9 should have 10% discount")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void Given_QuantityBetween4And9_When_GetDiscount_Then_Returns10Percent(int quantity)
    {
        // Act
        var discount = _policy.GetDiscount(quantity);

        // Assert
        discount.Should().Be(0.10m);
    }

    [Theory(DisplayName = "Quantities from 10 to 20 should have 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Given_QuantityBetween10And20_When_GetDiscount_Then_Returns20Percent(int quantity)
    {
        // Act
        var discount = _policy.GetDiscount(quantity);

        // Assert
        discount.Should().Be(0.20m);
    }

    [Theory(DisplayName = "Quantities above 20 should throw DomainException")]
    [InlineData(21)]
    [InlineData(50)]
    [InlineData(100)]
    public void Given_QuantityAbove20_When_ValidateQuantity_Then_ThrowsDomainException(int quantity)
    {
        // Act
        var act = () => _policy.ValidateQuantity(quantity);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }

    [Theory(DisplayName = "Quantities of zero or below should throw DomainException")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Given_QuantityZeroOrBelow_When_ValidateQuantity_Then_ThrowsDomainException(int quantity)
    {
        // Act
        var act = () => _policy.ValidateQuantity(quantity);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Theory(DisplayName = "Valid quantities should not throw")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    public void Given_ValidQuantity_When_ValidateQuantity_Then_DoesNotThrow(int quantity)
    {
        // Act
        var act = () => _policy.ValidateQuantity(quantity);

        // Assert
        act.Should().NotThrow();
    }
}
