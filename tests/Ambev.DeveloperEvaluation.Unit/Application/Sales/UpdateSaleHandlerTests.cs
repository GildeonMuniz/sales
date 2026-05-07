using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IPublisher _publisher;
    private readonly IMapper _mapper;
    private readonly IDiscountPolicy _discountPolicy;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _discountPolicy = new DefaultDiscountPolicy();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UpdateSaleProfile>());
        _mapper = config.CreateMapper();
        _handler = new UpdateSaleHandler(_repository, _publisher, _mapper, _discountPolicy);
    }

    [Fact(DisplayName = "Valid command should update sale fields and return mapped result")]
    public async Task Given_ValidCommand_When_Handle_Then_SaleIsUpdatedAndReturned()
    {
        // Arrange
        var existingSale = SaleHandlerTestData.GenerateSaleWithItems(_discountPolicy);
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(existingSale.Id);

        _repository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>())
            .Returns(existingSale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.CustomerName.Should().Be(command.CustomerName);
        result.BranchName.Should().Be(command.BranchName);
        await _repository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update should cancel old active items before adding new ones")]
    public async Task Given_SaleWithActiveItems_When_Updated_Then_OldItemsAreCancelled()
    {
        // Arrange
        var existingSale = SaleHandlerTestData.GenerateSaleWithItems(_discountPolicy, itemCount: 2);
        var originalItemIds = existingSale.Items.Select(i => i.Id).ToList();
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(existingSale.Id);

        _repository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>())
            .Returns(existingSale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        foreach (var id in originalItemIds)
            existingSale.Items.First(i => i.Id == id).IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Update should add new items from command")]
    public async Task Given_ValidCommand_When_Updated_Then_NewItemsAreAdded()
    {
        // Arrange
        var existingSale = SaleHandlerTestData.GenerateSaleWithItems(_discountPolicy);
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(existingSale.Id);
        command.Items.Add(new UpdateSaleItemCommand
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Extra Product",
            Quantity = 5,
            UnitPrice = 20m
        });

        _repository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>())
            .Returns(existingSale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var activeItems = existingSale.Items.Where(i => !i.IsCancelled).ToList();
        activeItems.Should().HaveCount(command.Items.Count);
    }

    [Fact(DisplayName = "Sale not found should throw KeyNotFoundException")]
    public async Task Given_NonExistentSaleId_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(Guid.NewGuid());
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.Id}*");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Empty Id should throw ValidationException")]
    public async Task Given_EmptyId_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(Guid.Empty);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Empty SaleNumber should throw ValidationException")]
    public async Task Given_EmptySaleNumber_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(Guid.NewGuid());
        command.SaleNumber = string.Empty;

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Command with no items should throw ValidationException")]
    public async Task Given_NoItems_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(Guid.NewGuid());
        command.Items.Clear();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one item*");
    }
}
