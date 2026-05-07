using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IPublisher _publisher;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CancelSaleItemHandler(_repository, _publisher);
    }

    [Fact(DisplayName = "Valid sale and item IDs should cancel item and return success")]
    public async Task Given_ValidSaleAndItemId_When_Handle_Then_ItemIsCancelledAndSuccessReturned()
    {
        // Arrange
        var policy = new DefaultDiscountPolicy();
        var sale = SaleHandlerTestData.GenerateSaleWithItems(policy);
        var itemId = sale.Items.First().Id;

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var command = new CancelSaleItemCommand(sale.Id, itemId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        sale.Items.First(i => i.Id == itemId).IsCancelled.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Non-existent sale ID should throw KeyNotFoundException")]
    public async Task Given_NonExistentSaleId_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _repository.GetByIdAsync(saleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var command = new CancelSaleItemCommand(saleId, itemId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{saleId}*");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Non-existent item ID should throw DomainException")]
    public async Task Given_NonExistentItemId_When_Handle_Then_ThrowsDomainException()
    {
        // Arrange
        var policy = new DefaultDiscountPolicy();
        var sale = SaleHandlerTestData.GenerateSaleWithItems(policy);
        var nonExistentItemId = Guid.NewGuid();

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, nonExistentItemId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"*{nonExistentItemId}*");
    }

    [Fact(DisplayName = "Cancelling item on already cancelled sale should throw DomainException")]
    public async Task Given_CancelledSale_When_Handle_Then_ThrowsDomainException()
    {
        // Arrange
        var policy = new DefaultDiscountPolicy();
        var sale = SaleHandlerTestData.GenerateSaleWithItems(policy);
        var itemId = sale.Items.First().Id;
        sale.Cancel();

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, itemId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already cancelled*");
    }

    [Fact(DisplayName = "Empty sale ID should throw ValidationException")]
    public async Task Given_EmptySaleId_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = new CancelSaleItemCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Sale ID is required*");
    }

    [Fact(DisplayName = "Empty item ID should throw ValidationException")]
    public async Task Given_EmptyItemId_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = new CancelSaleItemCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Item ID is required*");
    }
}
