using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _publisher = publisher;
    }

    public async Task<CancelSaleItemResponse> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found.");

        sale.CancelItem(request.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        foreach (var evt in sale.DomainEvents)
            await _publisher.Publish(evt, cancellationToken);
        sale.ClearDomainEvents();

        return new CancelSaleItemResponse { Success = true };
    }
}
