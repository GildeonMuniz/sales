using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(SaleCreatedEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[{EventName}] SaleId={SaleId} SaleNumber={SaleNumber} CustomerId={CustomerId} " +
            "CustomerName={CustomerName} Total={TotalAmount} OccurredAt={OccurredAt}",
            nameof(SaleCreatedEvent),
            evt.SaleId, evt.SaleNumber,
            evt.CustomerId, evt.CustomerName,
            evt.TotalAmount, evt.SaleDate);

        return Task.CompletedTask;
    }
}
