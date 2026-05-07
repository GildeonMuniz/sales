using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
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

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IPublisher _publisher;
    private readonly IMapper _mapper;
    private readonly IDiscountPolicy _discountPolicy;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _discountPolicy = new DefaultDiscountPolicy();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CreateSaleProfile>());
        _mapper = config.CreateMapper();
        _handler = new CreateSaleHandler(_repository, _publisher, _mapper, _discountPolicy);
    }

    [Fact(DisplayName = "Valid command should create sale and return mapped result")]
    public async Task Given_ValidCommand_When_Handle_Then_ReturnsMappedResult()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();

        _repository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _repository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.CustomerId.Should().Be(command.CustomerId);
        result.CustomerName.Should().Be(command.CustomerName);
        result.BranchId.Should().Be(command.BranchId);
        result.BranchName.Should().Be(command.BranchName);
        result.Items.Should().HaveCount(command.Items.Count);
        await _repository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Valid command should apply discount policy to items")]
    public async Task Given_ValidCommand_When_Handle_Then_DiscountIsApplied()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items[0].Quantity = 10; // 20% discount tier

        _repository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _repository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.First().Discount.Should().Be(0.20m);
    }

    [Fact(DisplayName = "Duplicate SaleNumber should throw ConflictException")]
    public async Task Given_DuplicateSaleNumber_When_Handle_Then_ThrowsConflictException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        var existing = new Sale(command.SaleNumber, DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");

        _repository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"*{command.SaleNumber}*");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Empty SaleNumber should throw ValidationException")]
    public async Task Given_EmptySaleNumber_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.SaleNumber = string.Empty;

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Empty CustomerName should throw ValidationException")]
    public async Task Given_EmptyCustomerName_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.CustomerName = string.Empty;

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Command with no items should throw ValidationException")]
    public async Task Given_NoItems_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items.Clear();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one item*");
    }

    [Fact(DisplayName = "Item quantity above 20 should throw DomainException")]
    public async Task Given_ItemQuantityAbove20_When_Handle_Then_ThrowsDomainException()
    {
        // Arrange
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items[0].Quantity = 21;

        _repository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
