namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesResult
{
    public IEnumerable<SaleSummaryResult> Items { get; set; } = Enumerable.Empty<SaleSummaryResult>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class SaleSummaryResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
