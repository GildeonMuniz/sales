using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the <see cref="SaleItemValidator"/> class.
/// Tests cover all SaleItem property validations.
/// </summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new();

    [Fact(DisplayName = "Valid item should pass all validation rules")]
    public void Given_ValidItem_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var policy = SaleTestData.GetDiscountPolicy();
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product A", 5, 10m, policy);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty ProductId should fail validation")]
    public void Given_EmptyProductId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange — create via reflection to bypass constructor validation
        var policy = SaleTestData.GetDiscountPolicy();
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product A", 5, 10m, policy);
        // Override via reflection to isolate validator logic from domain logic
        typeof(SaleItem)
            .GetProperty(nameof(SaleItem.ProductId))!
            .SetValue(item, Guid.Empty);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory(DisplayName = "Quantity exceeding 20 should fail validation")]
    [InlineData(21)]
    [InlineData(50)]
    public void Given_QuantityAbove20_When_Validated_Then_ShouldHaveError(int quantity)
    {
        // Arrange — bypass constructor, set quantity directly to test validator in isolation
        var item = CreateItemWithQuantity(quantity);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory(DisplayName = "Quantity of zero or below should fail validation")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Given_QuantityZeroOrBelow_When_Validated_Then_ShouldHaveError(int quantity)
    {
        // Arrange
        var item = CreateItemWithQuantity(quantity);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact(DisplayName = "Unit price of zero should fail validation")]
    public void Given_ZeroUnitPrice_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateItemWithPrice(0m);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact(DisplayName = "Negative unit price should fail validation")]
    public void Given_NegativeUnitPrice_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateItemWithPrice(-5m);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact(DisplayName = "Discount above 1 should fail validation")]
    public void Given_DiscountAbove1_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateItemWithDiscount(1.5m);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Discount);
    }

    [Fact(DisplayName = "Negative discount should fail validation")]
    public void Given_NegativeDiscount_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateItemWithDiscount(-0.1m);

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Discount);
    }

    // Helpers to create items with specific field values bypassing constructor constraints,
    // isolating the validator from domain rules already tested in SaleItemTests.

    private static SaleItem CreateItemWithQuantity(int quantity)
    {
        var policy = SaleTestData.GetDiscountPolicy();
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product", 5, 10m, policy);
        typeof(SaleItem).GetProperty(nameof(SaleItem.Quantity))!.SetValue(item, quantity);
        return item;
    }

    private static SaleItem CreateItemWithPrice(decimal price)
    {
        var policy = SaleTestData.GetDiscountPolicy();
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product", 5, 10m, policy);
        typeof(SaleItem).GetProperty(nameof(SaleItem.UnitPrice))!.SetValue(item, price);
        return item;
    }

    private static SaleItem CreateItemWithDiscount(decimal discount)
    {
        var policy = SaleTestData.GetDiscountPolicy();
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product", 5, 10m, policy);
        typeof(SaleItem).GetProperty(nameof(SaleItem.Discount))!.SetValue(item, discount);
        return item;
    }
}
