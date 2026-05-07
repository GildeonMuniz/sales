using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleReadRepository : ISaleReadRepository
{
    private readonly DefaultContext _context;

    public SaleReadRepository(DefaultContext context) => _context = context;

    public async Task<GetSaleResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new GetSaleResult
            {
                Id = s.Id,
                SaleNumber = s.SaleNumber,
                SaleDate = s.SaleDate,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                BranchId = s.BranchId,
                BranchName = s.BranchName,
                TotalAmount = s.TotalAmount,
                IsCancelled = s.IsCancelled,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                Items = s.Items.Select(i => new SaleItemResult
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    TotalAmount = i.TotalAmount,
                    IsCancelled = i.IsCancelled
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<SaleSummaryResult> Items, int TotalCount)> GetPagedAsync(
        int page, int size, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(s => new SaleSummaryResult
            {
                Id = s.Id,
                SaleNumber = s.SaleNumber,
                SaleDate = s.SaleDate,
                CustomerName = s.CustomerName,
                BranchName = s.BranchName,
                TotalAmount = s.TotalAmount,
                IsCancelled = s.IsCancelled
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
