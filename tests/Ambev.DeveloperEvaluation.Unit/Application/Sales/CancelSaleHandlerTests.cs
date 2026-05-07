using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
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

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IPublisher _publisher;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CancelSaleHandler(_repository, _publisher);
    }

    [Fact(DisplayName = "Valid sale ID should cancel sale and return success")]
    public async Task Given_ExistingSaleId_When_Handle_Then_SaleIsCancelledAndSuccessReturned()
    {
        // Arrange
        var policy = new DefaultDiscountPolicy();
        var sale = SaleHandlerTestData.GenerateSaleWithItems(policy);

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var command = new CancelSaleCommand(sale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        sale.IsCancelled.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.IsCancelled), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Non-existent sale ID should throw KeyNotFoundException")]
    public async Task Given_NonExistentSaleId_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var command = new CancelSaleCommand(id);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Empty sale ID should throw ValidationException")]
    public async Task Given_EmptySaleId_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = new CancelSaleCommand(Guid.Empty);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Sale ID is required*");
    }

    [Fact(DisplayName = "Already cancelled sale should throw DomainException")]
    public async Task Given_AlreadyCancelledSale_When_Handle_Then_ThrowsDomainException()
    {
        // Arrange
        var policy = new DefaultDiscountPolicy();
        var sale = SaleHandlerTestData.GenerateSaleWithItems(policy);
        sale.Cancel();

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already cancelled*");
    }
}
