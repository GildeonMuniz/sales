using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class GetSaleHandlerTests
{
    private readonly ISaleReadRepository _repository;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _repository = Substitute.For<ISaleReadRepository>();
        _handler = new GetSaleHandler(_repository);
    }

    [Fact(DisplayName = "Existing sale should be returned with all fields mapped")]
    public async Task Given_ExistingSaleId_When_Handle_Then_ReturnsMappedResult()
    {
        var id = Guid.NewGuid();
        var expected = new GetSaleResult
        {
            Id = id,
            SaleNumber = "S001",
            CustomerName = "John Doe",
            BranchName = "Main Branch",
            TotalAmount = 150m,
            IsCancelled = false,
            Items = new List<SaleItemResult> { new() { Id = Guid.NewGuid() } }
        };

        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetSaleQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(expected.Id);
        result.SaleNumber.Should().Be(expected.SaleNumber);
        result.CustomerName.Should().Be(expected.CustomerName);
        result.BranchName.Should().Be(expected.BranchName);
        result.TotalAmount.Should().Be(expected.TotalAmount);
        result.IsCancelled.Should().BeFalse();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Non-existent sale ID should throw KeyNotFoundException")]
    public async Task Given_NonExistentSaleId_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((GetSaleResult?)null);

        var act = async () => await _handler.Handle(new GetSaleQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Cancelled sale should be returned with IsCancelled true")]
    public async Task Given_CancelledSale_When_Handle_Then_ReturnsResultWithIsCancelledTrue()
    {
        var id = Guid.NewGuid();
        var expected = new GetSaleResult { Id = id, IsCancelled = true };

        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetSaleQuery(id), CancellationToken.None);

        result.IsCancelled.Should().BeTrue();
    }
}
