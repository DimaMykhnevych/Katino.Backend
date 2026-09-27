using Katino.Domain.Enums;

namespace Katino.Domain.Models.Statistics;

public class OrderStatusFilter
{
    public IReadOnlyCollection<OrderStatus> Statuses { get; }
    public bool IsExclusion { get; }

    private OrderStatusFilter(IReadOnlyCollection<OrderStatus> statuses, bool isExclusion)
    {
        Statuses = statuses;
        IsExclusion = isExclusion;
    }

    public static OrderStatusFilter Only(params OrderStatus[] statuses) => new(statuses, false);

    public static OrderStatusFilter Except(params OrderStatus[] statuses) => new(statuses, true);
}
