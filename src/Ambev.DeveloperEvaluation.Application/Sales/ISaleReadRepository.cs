using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public interface ISaleReadRepository
{
    Task<GetSaleResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<SaleSummaryResult> Items, int TotalCount)> GetPagedAsync(int page, int size, CancellationToken cancellationToken = default);
}
