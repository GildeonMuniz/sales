using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;

public static class SaleHandlerTestData
{
    private static readonly Faker Faker = new();

    public static CreateSaleCommand GenerateValidCreateCommand() => new()
    {
        SaleNumber = Faker.Random.AlphaNumeric(10).ToUpper(),
        SaleDate = Faker.Date.Recent(30),
        CustomerId = Guid.NewGuid(),
        CustomerName = Faker.Person.FullName,
        BranchId = Guid.NewGuid(),
        BranchName = Faker.Company.CompanyName(),
        Items =
        [
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = Faker.Commerce.ProductName(),
                Quantity = 5,
                UnitPrice = Math.Round(Faker.Random.Decimal(1, 100), 2)
            }
        ]
    };

    public static UpdateSaleCommand GenerateValidUpdateCommand(Guid saleId) => new()
    {
        Id = saleId,
        SaleNumber = Faker.Random.AlphaNumeric(10).ToUpper(),
        SaleDate = Faker.Date.Recent(30),
        CustomerId = Guid.NewGuid(),
        CustomerName = Faker.Person.FullName,
        BranchId = Guid.NewGuid(),
        BranchName = Faker.Company.CompanyName(),
        Items =
        [
            new UpdateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = Faker.Commerce.ProductName(),
                Quantity = 5,
                UnitPrice = Math.Round(Faker.Random.Decimal(1, 100), 2)
            }
        ]
    };

    public static Sale GenerateSaleWithItems(IDiscountPolicy policy, int itemCount = 1)
    {
        var sale = new Sale(
            Faker.Random.AlphaNumeric(10).ToUpper(),
            Faker.Date.Recent(30),
            Guid.NewGuid(),
            Faker.Person.FullName,
            Guid.NewGuid(),
            Faker.Company.CompanyName()
        );

        for (var i = 0; i < itemCount; i++)
            sale.AddItem(Guid.NewGuid(), Faker.Commerce.ProductName(), 5, 10m, policy);

        return sale;
    }
}
