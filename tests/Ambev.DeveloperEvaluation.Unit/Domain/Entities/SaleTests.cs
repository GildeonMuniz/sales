using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Ambev.DeveloperEvaluation.Domain.Entities.Sale"/> aggregate root.
/// Tests cover item management, total recalculation, cancellation rules and validation.
/// </summary>
public class SaleTests
{
    private readonly IDiscountPolicy _policy = SaleTestData.GetDiscountPolicy();

    [Fact(DisplayName = "Adding an item should recalculate the sale total")]
    public void Given_EmptySale_When_ItemAdded_Then_TotalAmountIsCalculated()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var (productId, productName, _, _) = SaleTestData.GenerateItemData();

        // Act
        sale.AddItem(productId, productName, 5, 10m, _policy);

        // Assert
        sale.TotalAmount.Should().Be(45m); // 5 * 10 * 0.90
        sale.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Adding the same product twice should merge quantities")]
    public void Given_ExistingProduct_When_SameProductAdded_Then_QuantitiesAreMerged()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var productId = Guid.NewGuid();

        sale.AddItem(productId, "Product X", 3, 10m, _policy);

        // Act — add the same product again (3+3=6, tier 10%)
        sale.AddItem(productId, "Product X", 3, 10m, _policy);

        // Assert
        sale.Items.Should().HaveCount(1);
        sale.Items.First().Quantity.Should().Be(6);
        sale.Items.First().Discount.Should().Be(0.10m);
    }

    [Fact(DisplayName = "Adding different products should create separate items")]
    public void Given_DifferentProducts_When_Added_Then_EachCreatesOwnItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", 2, 10m, _policy);
        sale.AddItem(Guid.NewGuid(), "Product B", 2, 20m, _policy);

        // Assert
        sale.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Adding item to cancelled sale should throw DomainException")]
    public void Given_CancelledSale_When_ItemAdded_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        // Act
        var (productId, productName, quantity, price) = SaleTestData.GenerateItemData();
        var act = () => sale.AddItem(productId, productName, quantity, price, _policy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cancelled sale*");
    }

    [Fact(DisplayName = "Cancelling a sale should set IsCancelled to true")]
    public void Given_ActiveSale_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.Cancel();

        // Assert
        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Cancelling an already cancelled sale should throw DomainException")]
    public void Given_CancelledSale_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        // Act
        var act = () => sale.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already cancelled*");
    }

    [Fact(DisplayName = "Cancelling an item should remove it from the sale total")]
    public void Given_SaleWithItems_When_ItemCancelled_Then_TotalIsRecalculated()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();

        sale.AddItem(productA, "Product A", 5, 10m, _policy); // 45.00
        sale.AddItem(productB, "Product B", 5, 10m, _policy); // 45.00
        var itemToCancel = sale.Items.First(i => i.ProductId == productA);

        // Act
        sale.CancelItem(itemToCancel.Id);

        // Assert
        sale.TotalAmount.Should().Be(45m);
        sale.Items.First(i => i.ProductId == productA).IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Cancelling a non-existent item should throw DomainException")]
    public void Given_InvalidItemId_When_CancelItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*not found*");
    }

    [Fact(DisplayName = "Cancelling an item from a cancelled sale should throw DomainException")]
    public void Given_CancelledSale_When_CancelItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(Guid.NewGuid(), "Product A", 5, 10m, _policy);
        var itemId = sale.Items.First().Id;
        sale.Cancel();

        // Act
        var act = () => sale.CancelItem(itemId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already cancelled sale*");
    }

    [Fact(DisplayName = "Valid sale should pass validation")]
    public void Given_ValidSale_When_Validated_Then_ShouldReturnValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(Guid.NewGuid(), "Product A", 2, 10m, _policy);

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Sale without items should fail validation")]
    public void Given_SaleWithNoItems_When_Validated_Then_ShouldReturnInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    // External Identities: IDs and name snapshots for Customer, Branch and Product
    // must be stored exactly as provided, decoupled from external domain state.

    [Fact(DisplayName = "Sale should preserve CustomerId and CustomerName snapshot (External Identity)")]
    public void Given_CustomerData_When_SaleCreated_Then_CustomerIdentityIsPreserved()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        const string customerName = "John Doe";
        var sale = new Sale("SALE001", DateTime.UtcNow, customerId, customerName, Guid.NewGuid(), "Test Branch");

        // Assert
        sale.CustomerId.Should().Be(customerId);
        sale.CustomerName.Should().Be(customerName);
    }

    [Fact(DisplayName = "Sale should preserve BranchId and BranchName snapshot (External Identity)")]
    public void Given_BranchData_When_SaleCreated_Then_BranchIdentityIsPreserved()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        const string branchName = "Downtown Branch";
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.NewGuid(), "Test Customer", branchId, branchName);

        // Assert
        sale.BranchId.Should().Be(branchId);
        sale.BranchName.Should().Be(branchName);
    }

    [Fact(DisplayName = "Added item should carry ProductId and ProductName snapshot (External Identity)")]
    public void Given_ProductData_When_ItemAdded_Then_ProductIdentityIsPreservedOnItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var productId = Guid.NewGuid();
        const string productName = "Premium Widget";

        // Act
        var item = sale.AddItem(productId, productName, 2, 10m, _policy);

        // Assert
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
    }

    [Fact(DisplayName = "Sale total should be sum of non-cancelled items only")]
    public void Given_SaleWithMixedItems_When_TotalCalculated_Then_OnlyActiveItemsCount()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();

        sale.AddItem(productA, "Active Product", 10, 10m, _policy);  // 80.00 (20% off)
        sale.AddItem(productB, "Cancelled Product", 5, 20m, _policy); // 90.00 (10% off)

        var itemToCancel = sale.Items.First(i => i.ProductId == productB);
        sale.CancelItem(itemToCancel.Id);

        // Assert
        sale.TotalAmount.Should().Be(80m);
    }
}
