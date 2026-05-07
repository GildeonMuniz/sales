using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleHandler : IRequestHandler<GetSaleQuery, GetSaleResult>
{
    private readonly ISaleReadRepository _saleReadRepository;

    public GetSaleHandler(ISaleReadRepository saleReadRepository)
    {
        _saleReadRepository = saleReadRepository;
    }

    public async Task<GetSaleResult> Handle(GetSaleQuery request, CancellationToken cancellationToken)
    {
        var result = await _saleReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (result is null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found.");

        return result;
    }
}
