using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly IMapper _mapper;
    private readonly IDiscountPolicy _discountPolicy;

    public UpdateSaleHandler(ISaleRepository saleRepository, IPublisher publisher, IMapper mapper, IDiscountPolicy discountPolicy)
    {
        _saleRepository = saleRepository;
        _publisher = publisher;
        _mapper = mapper;
        _discountPolicy = discountPolicy;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        sale.Update(command.SaleNumber, command.SaleDate, command.CustomerId, command.CustomerName, command.BranchId, command.BranchName);

        foreach (var activeItem in sale.Items.Where(i => !i.IsCancelled).ToList())
            sale.CancelItem(activeItem.Id);

        foreach (var item in command.Items)
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, _discountPolicy);

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        sale.ClearDomainEvents();

        await _publisher.Publish(
            new SaleModifiedEvent(updated.Id, updated.SaleNumber, updated.TotalAmount, updated.UpdatedAt ?? DateTime.UtcNow),
            cancellationToken);

        return _mapper.Map<UpdateSaleResult>(updated);
    }
}
