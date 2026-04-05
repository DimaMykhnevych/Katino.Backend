using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Helpers;

public class OrderStatusHelper
{
    private static readonly List<OrderStatus> PossibleManualOrderStatuses = [
            OrderStatus.Packed,
            OrderStatus.Refusal,
            OrderStatus.Exchange,
            OrderStatus.ReadyToShip,
        ];

    private static readonly List<OrderStatus> PossibleManualOrderStatusesForNonNp = [
            OrderStatus.Packed,
            OrderStatus.ReadyToShip,
            OrderStatus.Received,
            OrderStatus.Refusal,
            OrderStatus.Exchange,
        ];

    public static void SetOrderStatus(Order order, OrderStatus newOrderStatus, bool shouldPackedStatusBeUpdated)
    {
        if (newOrderStatus == OrderStatus.InProgress ||
           newOrderStatus == OrderStatus.ReadyToShip)
        {
            List<OrderStatus> possiblePreviousOrderStatuses = [
                OrderStatus.InProgress,
                OrderStatus.ReadyToShip,
                OrderStatus.None];

            if (possiblePreviousOrderStatuses.Contains(order.OrderStatus))
            {
                order.OrderStatus = newOrderStatus;
            }

            if (order.OrderStatus == OrderStatus.Packed && shouldPackedStatusBeUpdated)
            {
                order.OrderStatus = newOrderStatus;
            }
        }
    }

    public static bool ShouldUpdateToNpRelatedStatus(OrderStatus previousOrderStatus, OrderStatus newOrderStatus)
    {
        if (previousOrderStatus == OrderStatus.Packed && newOrderStatus != OrderStatus.Created)
        {
            return true;
        }

        if (previousOrderStatus == OrderStatus.Packed && newOrderStatus == OrderStatus.Created)
        {
            return false;
        }

        List<OrderStatus> orderStatusesThatCannotBeUpdated = [
            OrderStatus.InProgress,
            OrderStatus.ReadyToShip,
            OrderStatus.Refusal,
            OrderStatus.Exchange,
            ];

        if (!orderStatusesThatCannotBeUpdated.Contains(previousOrderStatus))
        {
            return true;
        }

        return false;
    }

    public static void ValidateManualOrderStatus(OrderStatus orderStatus)
    {
        if (!PossibleManualOrderStatuses.Contains(orderStatus))
        {
            throw new ArgumentException($"{orderStatus} cannot be manually set", nameof(orderStatus));
        }
    }

    public static void ValidateManualOrderStatusForNonNp(OrderStatus orderStatus)
    {
        if (!PossibleManualOrderStatusesForNonNp.Contains(orderStatus))
        {
            throw new ArgumentException($"{orderStatus} cannot be manually set for non-Nova Post orders", nameof(orderStatus));
        }
    }
}
