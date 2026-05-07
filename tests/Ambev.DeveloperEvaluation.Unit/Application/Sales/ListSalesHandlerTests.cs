using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class ListSalesHandlerTests
{
    private readonly ISaleReadRepository _repository;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _repository = Substitute.For<ISaleReadRepository>();
        _handler = new ListSalesHandler(_repository);
    }

    [Fact(DisplayName = "Valid pagination should return items and total count")]
    public async Task Given_ValidPagination_When_Handle_Then_ReturnsPagedResult()
    {
        var items = new List<SaleSummaryResult>
        {
            new() { Id = Guid.NewGuid(), SaleNumber = "S001" },
            new() { Id = Guid.NewGuid(), SaleNumber = "S002" }
        };

        _repository.GetPagedAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((items.AsEnumerable(), items.Count));

        var result = await _handler.Handle(new ListSalesQuery(1, 10), CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.Size.Should().Be(10);
    }

    [Fact(DisplayName = "Empty repository should return empty items list with total 0")]
    public async Task Given_EmptyRepository_When_Handle_Then_ReturnsEmptyResult()
    {
        _repository.GetPagedAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<SaleSummaryResult>(), 0));

        var result = await _handler.Handle(new ListSalesQuery(1, 10), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handler should forward page and size to repository")]
    public async Task Given_CustomPageAndSize_When_Handle_Then_RepositoryReceivesCorrectParams()
    {
        _repository.GetPagedAsync(3, 25, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<SaleSummaryResult>(), 0));

        await _handler.Handle(new ListSalesQuery(3, 25), CancellationToken.None);

        await _repository.Received(1).GetPagedAsync(3, 25, Arg.Any<CancellationToken>());
    }
}
