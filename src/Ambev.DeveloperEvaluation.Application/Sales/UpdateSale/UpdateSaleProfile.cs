using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<Sale, UpdateSaleResult>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
        CreateMap<SaleItem, SaleItemResult>();
    }
}
