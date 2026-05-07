using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<Sale, CreateSaleResult>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
        CreateMap<SaleItem, SaleItemResult>();
    }
}
