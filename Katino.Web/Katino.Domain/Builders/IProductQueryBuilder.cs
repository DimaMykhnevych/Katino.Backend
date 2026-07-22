using Katino.Domain.Entities;

namespace Katino.Domain.Builders;

public interface IProductQueryBuilder : IQueryBuilder<Product>
{
    IProductQueryBuilder SetBaseQuery();
    IProductQueryBuilder ApplySearch(string search);
    IProductQueryBuilder ApplyCategoryFilter(IEnumerable<Guid> categoryIds);
    IProductQueryBuilder ApplyDiscountFilter(bool? returnSpecificDiscountProducts);
    IProductQueryBuilder ApplyPaging(int page, int pageSize);
}
