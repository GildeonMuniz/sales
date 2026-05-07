using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the <see cref="SaleValidator"/> class.
/// Tests cover all Sale property validations.
/// </summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    [Fact(DisplayName = "Valid sale should pass all validation rules")]
    public void Given_ValidSale_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(Guid.NewGuid(), "Product", 2, 10m, SaleTestData.GetDiscountPolicy());

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Empty or missing sale number should fail validation")]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptySaleNumber_When_Validated_Then_ShouldHaveError(string? saleNumber)
    {
        // Arrange
        var sale = new Sale(saleNumber!, DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleNumber);
    }

    [Fact(DisplayName = "Sale number exceeding 50 characters should fail validation")]
    public void Given_LongSaleNumber_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = new Sale(SaleTestData.GenerateLongSaleNumber(), DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleNumber);
    }

    [Fact(DisplayName = "Empty CustomerId should fail validation")]
    public void Given_EmptyCustomerId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.Empty, "Customer", Guid.NewGuid(), "Branch");

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Theory(DisplayName = "Empty or missing customer name should fail validation")]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyCustomerName_When_Validated_Then_ShouldHaveError(string? name)
    {
        // Arrange
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.NewGuid(), name!, Guid.NewGuid(), "Branch");

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact(DisplayName = "Customer name exceeding 100 characters should fail validation")]
    public void Given_LongCustomerName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.NewGuid(), SaleTestData.GenerateLongCustomerName(), Guid.NewGuid(), "Branch");

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact(DisplayName = "Empty BranchId should fail validation")]
    public void Given_EmptyBranchId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.Empty, "Branch");

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchId);
    }

    [Theory(DisplayName = "Empty or missing branch name should fail validation")]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyBranchName_When_Validated_Then_ShouldHaveError(string? name)
    {
        // Arrange
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), name!);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchName);
    }

    [Fact(DisplayName = "Branch name exceeding 100 characters should fail validation")]
    public void Given_LongBranchName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = new Sale("SALE001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), SaleTestData.GenerateLongBranchName());

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchName);
    }

    [Fact(DisplayName = "Sale with no items should fail validation")]
    public void Given_SaleWithNoItems_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }
}
