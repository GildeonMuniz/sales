using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale aggregate operations.
/// </summary>
public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);

    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Sale> Items, int TotalCount)> GetPagedAsync(int page, int size, CancellationToken cancellationToken = default);
}
