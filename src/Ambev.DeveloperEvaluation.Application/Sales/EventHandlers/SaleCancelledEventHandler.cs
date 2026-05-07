using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
        => _logger = logger;

    public Task Handle(SaleCancelledEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[{EventName}] SaleId={SaleId} SaleNumber={SaleNumber} CancelledAt={CancelledAt}",
            nameof(SaleCancelledEvent),
            evt.SaleId, evt.SaleNumber, evt.CancelledAt);

        return Task.CompletedTask;
    }
}
