using Katino.Domain.Entities;

namespace Katino.Domain.Builders;

public interface IProductQueryBuilder : IQueryBuilder<Product>
{
    IProductQueryBuilder SetBaseQuery();
    IProductQueryBuilder ApplySearch(string search);
    IProductQueryBuilder ApplyPaging(int page, int pageSize);
}
