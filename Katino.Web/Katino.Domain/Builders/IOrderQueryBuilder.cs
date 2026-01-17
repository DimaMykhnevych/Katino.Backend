using Katino.Domain.Entities;

namespace Katino.Domain.Builders;

public interface IOrderQueryBuilder : IQueryBuilder<Order>
{
    IOrderQueryBuilder SetBaseOrderInfo();
    IOrderQueryBuilder SetBaseOrderInfoForToatalCount();
    IOrderQueryBuilder ApplyPaging(int page, int pageSize);
    IOrderQueryBuilder ApplySearch(string search);
}
