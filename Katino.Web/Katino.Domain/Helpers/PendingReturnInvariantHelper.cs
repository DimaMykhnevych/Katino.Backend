using Katino.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Katino.Domain.Helpers;

// Safety net for the invariant that must always hold for an order item:
// Quantity = RealCovered + PendingRemaining + QuantityToProduce.
// RealCovered isn't stored directly, so we check its consequence instead: the still-open pending
// (phantom) coverage can never exceed what's currently marked as covered (Quantity - QuantityToProduce).
public static class PendingReturnInvariantHelper
{
    public static void CheckOrderItemInvariant(OrderItem orderItem, int pendingRemaining, ILogger logger)
    {
        var covered = orderItem.Quantity - orderItem.QuantityToProduce;
        if (pendingRemaining > covered)
        {
            logger?.LogError(
                "Pending-return invariant violated for order item {OrderItemId}: pendingRemaining={PendingRemaining} exceeds covered={Covered} (Quantity={Quantity}, QuantityToProduce={QuantityToProduce})",
                orderItem.Id, pendingRemaining, covered, orderItem.Quantity, orderItem.QuantityToProduce);
        }
    }
}
