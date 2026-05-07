using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly IMapper _mapper;
    private readonly IDiscountPolicy _discountPolicy;

    public CreateSaleHandler(ISaleRepository saleRepository, IPublisher publisher, IMapper mapper, IDiscountPolicy discountPolicy)
    {
        _saleRepository = saleRepository;
        _publisher = publisher;
        _mapper = mapper;
        _discountPolicy = discountPolicy;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existing = await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"Sale with number '{command.SaleNumber}' already exists.");

        var sale = new Sale(command.SaleNumber, command.SaleDate, command.CustomerId, command.CustomerName, command.BranchId, command.BranchName);

        foreach (var item in command.Items)
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, _discountPolicy);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _publisher.Publish(
            new SaleCreatedEvent(created.Id, created.SaleNumber, created.SaleDate,
                created.CustomerId, created.CustomerName, created.TotalAmount),
            cancellationToken);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
