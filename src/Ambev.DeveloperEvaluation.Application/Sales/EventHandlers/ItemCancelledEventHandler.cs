using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
        => _logger = logger;

    public Task Handle(ItemCancelledEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[{EventName}] SaleId={SaleId} ItemId={ItemId} ProductId={ProductId} " +
            "ProductName={ProductName} CancelledAt={CancelledAt}",
            nameof(ItemCancelledEvent),
            evt.SaleId, evt.ItemId,
            evt.ProductId, evt.ProductName, evt.CancelledAt);

        return Task.CompletedTask;
    }
}
