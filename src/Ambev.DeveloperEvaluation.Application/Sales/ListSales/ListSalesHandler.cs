using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleReadRepository _saleReadRepository;

    public ListSalesHandler(ISaleReadRepository saleReadRepository)
    {
        _saleReadRepository = saleReadRepository;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _saleReadRepository.GetPagedAsync(request.Page, request.Size, cancellationToken);

        return new ListSalesResult
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            Size = request.Size
        };
    }
}
