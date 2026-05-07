using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
        => _logger = logger;

    public Task Handle(SaleModifiedEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[{EventName}] SaleId={SaleId} SaleNumber={SaleNumber} " +
            "Total={TotalAmount} ModifiedAt={ModifiedAt}",
            nameof(SaleModifiedEvent),
            evt.SaleId, evt.SaleNumber,
            evt.TotalAmount, evt.ModifiedAt);

        return Task.CompletedTask;
    }
}
