using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for Sale and SaleItem entities.
/// Centralizes all test data generation to ensure consistency across test cases.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a valid Sale with no items.
    /// </summary>
    public static Sale GenerateValidSale() => new Sale(
        Faker.Random.AlphaNumeric(10).ToUpper(),
        Faker.Date.Recent(30),
        Guid.NewGuid(),
        Faker.Person.FullName,
        Guid.NewGuid(),
        Faker.Company.CompanyName()
    );

    /// <summary>
    /// Returns a real DefaultDiscountPolicy for use in entity tests.
    /// </summary>
    public static IDiscountPolicy GetDiscountPolicy() => new DefaultDiscountPolicy();

    /// <summary>
    /// Generates a valid product tuple for use in AddItem calls.
    /// </summary>
    public static (Guid ProductId, string ProductName, int Quantity, decimal UnitPrice) GenerateItemData(int quantity = 5)
    {
        return (Guid.NewGuid(), Faker.Commerce.ProductName(), quantity, Math.Round(Faker.Random.Decimal(1, 100), 2));
    }

    public static string GenerateInvalidSaleNumber() => string.Empty;

    public static string GenerateLongSaleNumber() => Faker.Random.String2(51);

    public static string GenerateLongCustomerName() => Faker.Random.String2(101);

    public static string GenerateLongBranchName() => Faker.Random.String2(101);
}
