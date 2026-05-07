using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;

    public CancelSaleHandler(ISaleRepository saleRepository, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _publisher = publisher;
    }

    public async Task<CancelSaleResponse> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found.");

        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        await DispatchDomainEventsAsync(sale.DomainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return new CancelSaleResponse { Success = true };
    }

    private async Task DispatchDomainEventsAsync(IEnumerable<INotification> events, CancellationToken ct)
    {
        foreach (var evt in events)
            await _publisher.Publish(evt, ct);
    }
}
