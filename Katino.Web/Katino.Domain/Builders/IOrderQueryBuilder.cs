using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Builders;

public interface IOrderQueryBuilder : IQueryBuilder<Order>
{
    IOrderQueryBuilder SetBaseOrderInfo();
    IOrderQueryBuilder SetBaseOrderInfoForToatalCount();
    IOrderQueryBuilder ApplyPaging(int page, int pageSize);
    IOrderQueryBuilder ApplySearch(string search);
    IOrderQueryBuilder ApplyOrderStatusFilter(IList<OrderStatus> orderStatuses);
    IOrderQueryBuilder ApplyTagFilter(IList<Guid> tagIds);
    IOrderQueryBuilder ApplyOrderCreationDateFilter(DateTimeOffset? from, DateTimeOffset? to);
    IOrderQueryBuilder ApplySorting(OrderSort sort);
}
