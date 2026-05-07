using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the <see cref="SaleItem"/> entity.
/// Tests cover total calculation, discount application, cancellation and update rules.
/// </summary>
public class SaleItemTests
{
    private readonly IDiscountPolicy _policy = SaleTestData.GetDiscountPolicy();

    [Fact(DisplayName = "Item with quantity below 4 should have no discount")]
    public void Given_QuantityBelow4_When_Created_Then_DiscountIsZeroAndTotalIsCorrect()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const int quantity = 3;
        const decimal unitPrice = 10m;

        // Act
        var item = new SaleItem(Guid.NewGuid(), productId, "Product A", quantity, unitPrice, _policy);

        // Assert
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(30m);
    }

    [Fact(DisplayName = "Item with quantity 4 to 9 should have 10% discount")]
    public void Given_QuantityBetween4And9_When_Created_Then_Discount10PercentApplied()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const int quantity = 5;
        const decimal unitPrice = 10m;

        // Act
        var item = new SaleItem(Guid.NewGuid(), productId, "Product B", quantity, unitPrice, _policy);

        // Assert
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(45m); // 5 * 10 * 0.90
    }

    [Fact(DisplayName = "Item with quantity 10 to 20 should have 20% discount")]
    public void Given_QuantityBetween10And20_When_Created_Then_Discount20PercentApplied()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const int quantity = 10;
        const decimal unitPrice = 10m;

        // Act
        var item = new SaleItem(Guid.NewGuid(), productId, "Product C", quantity, unitPrice, _policy);

        // Assert
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(80m); // 10 * 10 * 0.80
    }

    [Fact(DisplayName = "Item with quantity above 20 should throw DomainException on creation")]
    public void Given_QuantityAbove20_When_Created_Then_ThrowsDomainException()
    {
        // Act
        var act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product D", 21, 10m, _policy);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Valid item should be cancelled successfully")]
    public void Given_ActiveItem_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product E", 5, 10m, _policy);

        // Act
        item.Cancel();

        // Assert
        item.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Already cancelled item should throw DomainException when cancelled again")]
    public void Given_CancelledItem_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product F", 5, 10m, _policy);
        item.Cancel();

        // Act
        var act = () => item.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already cancelled*");
    }

    [Fact(DisplayName = "Cancelled item should throw DomainException when updated")]
    public void Given_CancelledItem_When_Updated_Then_ThrowsDomainException()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product G", 5, 10m, _policy);
        item.Cancel();

        // Act
        var act = () => item.Update(3, 10m, _policy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cancelled*");
    }

    [Fact(DisplayName = "Update should recalculate discount and total correctly")]
    public void Given_ActiveItem_When_Updated_Then_TotalsAreRecalculated()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Product H", 3, 10m, _policy);

        // Act — update to quantity 10 (20% discount tier)
        item.Update(10, 20m, _policy);

        // Assert
        item.Quantity.Should().Be(10);
        item.UnitPrice.Should().Be(20m);
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(160m); // 10 * 20 * 0.80
    }

    // External Identities: product ID and name snapshot must be stored as-is,
    // independent of any external product catalog state.

    [Fact(DisplayName = "Item should store the exact ProductId provided (External Identity)")]
    public void Given_ProductId_When_ItemCreated_Then_ProductIdIsPreserved()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = new SaleItem(Guid.NewGuid(), productId, "Product X", 5, 10m, _policy);

        // Assert
        item.ProductId.Should().Be(productId);
    }

    [Fact(DisplayName = "Item should store the product name snapshot provided (External Identity)")]
    public void Given_ProductName_When_ItemCreated_Then_ProductNameSnapshotIsPreserved()
    {
        // Arrange
        const string productName = "Super Widget Pro";

        // Act
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), productName, 5, 10m, _policy);

        // Assert
        item.ProductName.Should().Be(productName);
    }

    [Fact(DisplayName = "Updating item should not change the product identity snapshot")]
    public void Given_ExistingItem_When_Updated_Then_ProductIdentityRemainsUnchanged()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string productName = "Original Name";
        var item = new SaleItem(Guid.NewGuid(), productId, productName, 3, 10m, _policy);

        // Act
        item.Update(5, 15m, _policy);

        // Assert
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
    }
}
