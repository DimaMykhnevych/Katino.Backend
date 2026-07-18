# Incoming Return Tracking ("Pending Physical Arrival")

## Status

Implemented and reviewed in this repository. Migrations for this feature are managed manually by the
repo owner (not committed via `dotnet ef migrations add` automatically) - if you are an agent picking up
work here, **check with the user before assuming any related migration exists or has been applied.**

## Why this feature exists

When an order is rejected, its stock doesn't return to the warehouse instantly - the package is
physically in transit back. To avoid blocking fulfillment, the system immediately (optimistically)
reassigns that quantity to the next most urgent order that needs it, marking that order's item as
`Ready` even though nothing has physically arrived yet.

This creates an accounting problem: an order can look "ready to ship" while actually waiting on a
package that hasn't arrived, and might never arrive in time. Seamstresses need to see this ("this
product is coming from a return - if it doesn't show up before the ship date, sew a replacement"), and
the whole system needs to correctly track, resolve, and re-route this "still in transit" quantity as
orders get edited, deleted, urgently reprioritized, or as seamstresses proactively sew replacements.

This document describes the full design: the data model, the invariant that must always hold, every
code path that creates/consumes/forwards a "still in transit" claim, and a list of bugs that were found
and fixed while building this (useful context for anyone touching this code later - these bugs are easy
to reintroduce if the underlying reasoning isn't understood).

## Vocabulary

- **Real coverage** - an order item's need is satisfied by stock that genuinely, physically exists
  (already sewn, or already in the warehouse). Not separately recorded anywhere; it's whatever portion of
  `Quantity - QuantityToProduce` is *not* backed by an open pending-return row (see invariant below).
- **Phantom coverage / "still in transit" / "pending physical arrival"** - an order item's need is
  satisfied only on paper, because quantity was virtually redistributed to it from a source that hasn't
  physically delivered yet (a rejected order's return, or quantity passed along from another order that
  itself had phantom coverage). Tracked explicitly via `ProductVariantRedistributionHistory` rows with
  `IsPendingPhysicalArrival = true`.
- **The ledger** - the `ProductVariantRedistributionHistory` table, used both as an audit trail (every
  quantity movement, `Reason`-tagged) and, for phantom rows specifically, as a live tracking mechanism for
  "how much is still owed and to whom."
- **Donor** - in urgent reallocation, the less-urgent order an urgent order takes stock from.
- **Recorder** - `ProductVariantRedistributionRecorder`, the single place that writes ledger rows and
  attaches/relies on the `PendingIncomingReturn` order tag.

## The core invariant

For any (non-custom) `OrderItem`, at all times:

```
Quantity = RealCovered + PendingRemaining + QuantityToProduce
```

- `Quantity` - total units needed for this order item. Never changes after creation.
- `RealCovered` - not stored anywhere directly; it's whatever is covered but not backed by an open
  pending-return row. Computed implicitly as `Quantity - QuantityToProduce - PendingRemaining`.
- `PendingRemaining` - `GetPendingReturnRemainingAsync(orderItemId)`: sum of `Quantity - QuantityResolved`
  across every open (`IsPendingPhysicalArrival = true`) ledger row targeting this item.
- `QuantityToProduce` - the existing field on `OrderItem`. Represents "not covered by *anything* yet,
  real or phantom" - must genuinely be produced (by regular sewing or some future redistribution) with no
  fallback.

**Consequence that surprises people at first**: `QuantityToProduce` and `PendingRemaining` are two
disjoint, non-overlapping slices of the same total need. An order item can legitimately show up in the
sewing queue *twice* - once as a normal "must sew, no fallback" line (`QuantityToProduce`), and once as an
"incoming return, might not need sewing if the package arrives" line (`PendingRemaining`). This is not a
bug or a duplicate; it's two different signals about two different physical units.

`PendingReturnInvariantHelper.CheckOrderItemInvariant(orderItem, pendingRemaining, logger)`
(`Katino.Domain/Helpers/PendingReturnInvariantHelper.cs`) is a safety net, not a hard guarantee: since
`RealCovered` isn't stored, it checks the only checkable consequence - `pendingRemaining` can never exceed
`covered = Quantity - QuantityToProduce`. If it does, something in the code has double-counted or lost
track of a released/consumed amount; it logs `LogError` (never throws - this is diagnostic, not a gate).
It's called after essentially every place that mutates an item's `QuantityToProduce` or consumes/releases
part of its pending ledger:
- `UpdateProductVariantService.HandleProductVariantQuantityChange` (per item, per assignment)
- `UrgentOrderRedistributionService.RedistributeForUrgentOrderAsync` (per touched item, at the end)
- `OrderItemChangeService.HandleUpdatedOrderItems` (per item, right after its new status is computed)

If you add a new code path that changes `QuantityToProduce` or touches the pending ledger for an item,
add a call here too.

## Data model

### `ProductVariantRedistributionHistory` (`Katino.Domain/Entities/ProductVariantRedistributionHistory.cs`)

The append-mostly ledger of every quantity movement.

| Field | Meaning |
|---|---|
| `ProductVariantId` | which product variant |
| `Reason` | `ProductVariantQuantityChangeReason` - **audit/display only**, never drives logic. See below. |
| `Quantity` | how much this row moved. Immutable historical fact - never mutated after insert. |
| `IsPendingPhysicalArrival` | **the only field that drives tag/queue/ledger logic.** True = this row's remaining quantity is still physically in transit, not confirmed real. |
| `QuantityResolved` | how much of `Quantity` has been closed out - either by sewing a replacement, or by being handed off to a different order/item. Remaining open balance = `Quantity - QuantityResolved`. |
| `SourceOrderId` / `SourceOrderItemId` / `SourceOrderTtnSnapshot` | where the quantity came from, when known/relevant (donor order for `UrgentReallocation`, rejected order for `OrderRejected`, etc). |
| `TargetOrderId` / `TargetOrderItemId` | who received it. Null `TargetOrderId` means "went to generic stock." |
| `CreatedAtUtc` | used for FIFO ordering when consuming a multi-row ledger for one item. |

`Reason` values (`Katino.Domain/Enums/ProductVariantQuantityChangeReason.cs`): `ManualEdit`,
`OrderRejected`, `OrderDeleted`, `OrderEdited`, `Sewing`, `UrgentReallocation`, `ReturnCoveredBySewing`.
**Do not branch logic on `Reason`.** It used to be (`Reason == OrderRejected`) the tag/logic trigger; this
was wrong because the same reason can carry both phantom and real quantity (`OrderDeleted`/`OrderEdited`),
and other reasons (`UrgentReallocation`) can carry phantom quantity too. `IsPendingPhysicalArrival` is the
decoupled, correct flag for that.

`ReturnCoveredBySewing` exists purely as an audit-trail marker: "this order's need was covered by a
sewn replacement instead of waiting for the return." It is never used with `IsPendingPhysicalArrival =
true` for the row it's recorded on (that row is terminal/resolved); it's used with
`IsPendingPhysicalArrival = true` for the *separate* forwarding event that hands the freed-up physical
shipment to the next order (see "Sewing a replacement" below) - there, `Reason` is set to
`ReturnCoveredBySewing` purely for readability in the history view ("this arrived here because someone
else sewed a replacement instead of waiting"), not `OrderRejected`.

### `SewingQueueItem` (`Katino.Domain/Models/SewingQueueItem.cs`)

Unified queue row model, used for normal, custom, and incoming-return items alike.

```csharp
public class SewingQueueItem
{
    public Guid ProductVariantId { get; init; }
    public ProductVariant ProductVariant { get; init; }
    public int QuantityToProduce { get; init; }     // meaning depends on IsIncomingReturn - see below
    public bool IsCustomTailoring { get; init; }
    public bool IsIncomingReturn { get; init; }
    public DateTime? SendUntil { get; init; }        // set only when IsIncomingReturn = true
    public string Comment { get; init; }
    public Guid? OrderItemId { get; init; }
}
```

For `IsIncomingReturn = true` rows, `QuantityToProduce` actually holds `PendingRemaining` (the open
ledger amount), not the item's real `QuantityToProduce` field - the field is reused rather than adding a
separate one, since the two never appear together in the same row (a single row is either a normal
"must-sew" line or an "incoming-return, might-not-need-sewing" line - see the queue-merging logic below
for how both can appear for the same underlying `OrderItem`).

### `PendingIncomingReturnSummary` (`Katino.Domain/Models/PendingIncomingReturnSummary.cs`)

One row per order item, pre-aggregated: `ProductVariantId`, `ProductVariant`, `TargetOrderId`,
`TargetOrderItemId`, `SendUntilDate`, `RemainingQuantity`. This is what
`GetPendingIncomingReturnsAsync` returns - it exists so an item covered by several partial pending-return
rows still surfaces as exactly one queue line, never duplicated.

### `SewingQueueGroupingHelper` (`Katino.Domain/Helpers/SewingQueueGroupingHelper.cs`)

Two static methods: `Group(IEnumerable<SewingQueueItem>)` and
`GroupByDate(IEnumerable<(DateTime Date, SewingQueueItem Item)>)`.

Rule: items where `IsCustomTailoring || IsIncomingReturn` are **never** grouped by `ProductVariantId`
across different orders (each stays its own row, tied to its specific `OrderItemId`, because custom items
are order-specific by nature and incoming-return items must stay traceable back to a specific order's
pending ledger for later consumption). Everything else is summed by `ProductVariantId` across orders,
because for plain sewing it doesn't matter which order eventually receives the output.

## The write path: who creates/consumes/forwards phantom quantity, and why

### 1. Order rejected → phantom is born

`DeleteOrderService.HandleOrderRejectionAsync` (triggered from `SetOrderManualStatusService` for manual
`Refusal`/`Exchange`, or from the NP webhook flow) computes the freed quantity via
`OrderItemChangeService.HandleOrderItemsReturn` → `HandleDeletedOrderItems` (with `deleteOrderItems:
false` - items stay in the DB, only their effect on stock is processed), then calls:

```csharp
await _updateProductVariantService.HandleProductVariantQuantityChange(
    productVariantId, updatedQuantity, order.Id,
    ProductVariantQuantityChangeReason.OrderRejected,
    order.InternetDocumentIntDocNumber,
    isPendingPhysicalArrival: true);
```

**No split here.** The whole freed batch is treated as 100% phantom, regardless of whether the rejected
order's own coverage was itself real or already-phantom - either way, nothing from a rejected order is
physically back in the warehouse yet, so everything redistributed from it must be flagged pending.

`ProductVariantRedistributionRecorder.RecordAsync` sees `IsPendingPhysicalArrival = true` on the event and
attaches the `PendingIncomingReturn` order tag to every `TargetOrderId` in the resulting lines
(`Katino.Infrastructure/Persistance/Services/ProductVariantRedistribution/ProductVariantRedistributionRecorder.cs`).
This is the **only** place that ever attaches the tag; every other path only ever detaches it once its
own open ledger is confirmed empty.

### 2. Seamstress sees it in the queue

`SewingQueueService.GetSewingQueueAsync` / `GetSewingQueueGroupedByDateAsync`
(`Katino.Infrastructure/Persistance/Services/OrderItem/SewingQueueService.cs`) merge two sources:

- `IOrderItemRepository.GetOrderItemsForSewing...Async` - normal `ForSewing` items (unconditional need).
- `IProductVariantRedistributionHistoryRepository.GetPendingIncomingReturnsAsync(sewerId)` - incoming
  return items (conditional need, see invariant above).

Both go through the same sewer-visibility filter (`ProductVariant.SewingQueueVisibility`/`Sewers`) and the
same `SewingQueueGroupingHelper`. `GetPendingIncomingReturnsAsync`
(`Katino.Infrastructure/Persistance/Repositories/ProductVariantRedistributionHistoryRepository/ProductVariantRedistributionHistoryRepository.cs`)
filters `IsPendingPhysicalArrival AND TargetOrderItemId != null AND Quantity > QuantityResolved AND
TargetOrder has the PendingIncomingReturn tag`, applies the same sewer-visibility filter, then groups by
`TargetOrderItemId` and sums the remaining quantity - producing `PendingIncomingReturnSummary` rows (one
per item, never duplicated even if the item has several open ledger rows).

There's also a standalone `IncomingReturnQueueService` /
`GET /api/OrderItem/incoming-return-queue` endpoint that surfaces *only* the incoming-return slice,
independent of the merged queue - kept around in case it's useful separately, not wired into anything
else.

### 3. Seamstress sews a replacement instead of waiting

`SewingProductionReportService.ApplySewedAsync` branches on `orderItem.IsCustomTailoring` (not on whether
`OrderItemId` was passed - that used to be the branch condition and was wrong once incoming-return items
also started carrying an `OrderItemId`; it also used to make `SewingHistory.IsCustomTailoring` wrong for
the same reason).

Three branches:
- `OrderItemId == null` → `ApplyRegularSewedAsync` - unaddressed production, distributed by urgency via
  `HandleProductVariantQuantityChange(Reason=Sewing)`, unrelated to any specific order.
- `IsCustomTailoring == true` → `ApplyCustomSewedAsync` - unchanged from before this feature, decrements
  `QuantityToProduce` directly for that one order item.
- `IsCustomTailoring == false` but `OrderItemId` given → `ApplyIncomingReturnSewedAsync` (new).

```csharp
private async Task ApplyIncomingReturnSewedAsync(OrderItem orderItem, Guid productVariantId, int actualSewedQuantity)
{
    var remaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(orderItem.Id);
    if (remaining <= 0) throw ...;              // not actually a pending-return item
    if (actualSewedQuantity > remaining) throw ...;

    await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();
    try
    {
        await _redistributionHistoryRepository.ConsumePendingReturnAsync(orderItem.Id, actualSewedQuantity);

        await _redistributionRecorder.RecordAsync(new ProductVariantRedistributionEvent
        {
            Reason = ProductVariantQuantityChangeReason.ReturnCoveredBySewing,
            Lines = [ new() { TargetOrderId = orderItem.OrderId, TargetOrderItemId = orderItem.Id, Quantity = actualSewedQuantity } ]
            // IsPendingPhysicalArrival left false (default) - this row is a terminal audit record.
        });

        var pv = await _productVariantRepository.Get(productVariantId);
        await _updateProductVariantService.HandleProductVariantQuantityChange(
            productVariantId, pv.QuantityInStock + actualSewedQuantity,
            orderIdToSkipFromProcessing: orderItem.OrderId,
            reason: ProductVariantQuantityChangeReason.ReturnCoveredBySewing,
            sourceOrderTtnSnapshot: orderItem.Order.InternetDocumentIntDocNumber,
            isPendingPhysicalArrival: true);          // <-- still phantom, forwarded onward

        // invariant check, then: if no other open pending rows for orderItem.OrderId, detach the tag
    }
    catch { rollback; throw; }
}
```

**Critical point, easy to get backwards**: `orderItem.QuantityToProduce` / `OrderItemStatus` are **never
touched** in this method. By the invariant, closing a pending-return promise with a sewn replacement does
not change `QuantityToProduce` - it converts part of `PendingRemaining` into `RealCovered`, and
`QuantityToProduce` (the genuinely-uncovered remainder, if any - e.g. left over from an earlier urgent
reallocation "theft," see below) is completely unaffected. **If you find yourself wanting to touch
`QuantityToProduce` here, you are re-deriving the exact mistake that was made and corrected mid-design -
see "Bugs found and fixed" below, item 3.**

There are two separate physical flows happening here, easy to conflate: (1) the seamstress's freshly-sewn
garment goes *directly* to this order item, silently, no ledger row needed for that (it's just real
production, same as any other sewing); (2) the *old, still-physically-incoming* shipment that this item no
longer needs is a completely separate thing, and *that* is what gets forwarded via the
`HandleProductVariantQuantityChange(..., isPendingPhysicalArrival: true)` call, `Quantity`-for-`Quantity`
equal to what was just sewn (since sewing X units frees up exactly X units' worth of no-longer-needed
incoming shipment) but not literally the same physical items.

### 4. Urgent reallocation - a more urgent order steals from a less urgent donor

`UrgentOrderRedistributionService.RedistributeForUrgentOrderAsync`
(`Katino.Infrastructure/Persistance/Services/Order/UrgentOrderRedistributionService.cs`). For each item
the urgent order still needs:

1. Build a candidate list from **every** matching order item across every less-urgent donor order (not
   just the first match per donor order - a single donor order can have two items of the same product
   variant, one real-covered and one phantom-covered, and both must be visible as separate candidates; see
   "Bugs found and fixed" item 1).
2. For each candidate item, `phantomAvailable = min(GetPendingReturnRemainingAsync(item), readyQuantity)`,
   `realAvailable = readyQuantity - phantomAvailable`.
3. Two passes: first take `phantomAvailable` from **every** candidate (in donor-priority order), then -
   only if the urgent item still needs more - take `realAvailable` from every candidate. This means: if
   the highest-priority donor has no phantom coverage but a lower-priority donor does, the phantom source
   is preferred even though it's a lower-priority donor by the normal urgency ranking. This is intentional
   (confirmed with the repo owner): taking a phantom claim away costs nothing extra (the donor was never
   going to have real stock for it anyway, it just needs to keep waiting or get a replacement sewn like
   anyone else), while taking real stock forces the donor to genuinely re-produce something it already
   had. Minimizing forced re-production system-wide is worth reordering donor priority for.
4. For each phantom take: `ConsumePendingReturnAsync(donorItem.Id, quantityToTake)`, then record a new
   ledger row `Reason = UrgentReallocation, IsPendingPhysicalArrival = true, Source = donorOrder/donorItem,
   Target = urgentOrder/urgentItem` - the Recorder auto-tags the urgent order from this.
5. For each real take: same but `IsPendingPhysicalArrival = false` - no tag.
6. After all items are processed: for every donor whose phantom coverage was (partially or fully) taken,
   check `HasOpenPendingReturnsForOrderAsync(donorOrder.Id)` and detach the tag if nothing else is open.
7. Invariant check on every touched item (donors and the urgent item itself).

### 5. Order deleted or edited - the freed quantity can be a mix of real and phantom

Both `DeleteOrderService.DeleteAsync` and `UpdateOrderService.UpdateAsync` funnel through
`OrderItemChangeService`, which (via `ProcessOrderItemProductVariantDeletion`, private, shared by both
"true delete" and "the old half of an update") computes, per touched item:

```csharp
var freedQuantity = status switch { Ready => Quantity, ForSewing => Quantity - QuantityToProduce, _ => 0 };
if (freedQuantity > 0)
{
    var phantomRemaining = await GetPendingReturnRemainingAsync(orderItem.Id);
    // Only release pending coverage for the part of the reduction that eats into what was actually
    // covered (freedQuantity) - not the raw Quantity delta. The already-uncovered part
    // (QuantityToProduce) has nothing to release; shrinking it just needs less production. See "Bugs
    // found and fixed" item 6.
    var coverageReduction = Math.Max(0, freedQuantity - survivingQuantity);   // survivingQuantity: see below
    var phantomPart = Math.Min(coverageReduction, phantomRemaining);
    if (phantomPart > 0)
    {
        await ConsumePendingReturnAsync(orderItem.Id, phantomPart);
        phantomQuantitiesFreed[productVariant.Id] += phantomPart;
    }
    productVariant.QuantityInStock += freedQuantity;   // full amount, unconditionally - see note below
}
```

`survivingQuantity` (default `0`, meaning "this item is genuinely gone") is only passed a non-zero value
from `HandleUpdatedOrderItems`, and only when the product variant on the line didn't change:

```csharp
var survivingQuantity = item.ProductVariantId == existingOrderItem.ProductVariantId ? item.Quantity : 0;
await ProcessOrderItemProductVariantDeletion(..., existingOrderItem, ..., survivingQuantity);
```

This exists because an *update* re-uses the same `OrderItem.Id` (quantity changes, the row doesn't get
replaced) - so if only *part* of the item's need goes away, the rest must stay attached, with its own
pending-return coverage (if any) left open on the same row, not force-closed. See "Bugs found and fixed"
items 4 and 6 for the concrete scenarios that motivated this and its correction.

Note the deliberate asymmetry: `productVariant.QuantityInStock += freedQuantity` always adds the *entire*
freed amount to the local snapshot (used purely to compute the new item's Ready/ForSewing status via
`ProcessExistingOrderItemsStatus`, and later the order-level `totalDelta`), regardless of
`survivingQuantity`/`coverageReduction`. Only the *pending-ledger consumption* is capped by
`coverageReduction`. This is intentional and was verified carefully - the local stock-pool arithmetic is a
separate, already-correct mechanism for "how much total moves"; the fix only had to correct *which
specific ledger row(s)* get
closed and by how much, not the pool math around it.

Back in `DeleteOrderService.DeleteAsync` / `UpdateOrderService.UpdateAsync`, after all items are
processed:

```csharp
var totalDelta = updatedQuantity - currentQuantity.Value;
var phantomDelta = Math.Min(phantomQuantitiesFreed.GetValueOrDefault(pv), totalDelta);
var realDelta = totalDelta - phantomDelta;
await _updateProductVariantService.HandleProductVariantQuantityChangeSplit(
    pv, currentQuantity.Value, realDelta, phantomDelta, orderId, reason, ttnSnapshot);
```

`HandleProductVariantQuantityChangeSplit`
(`Katino.Infrastructure/Persistance/Services/ProductVariant/UpdateProductVariantService.cs`) runs **two
sequential passes** through the normal urgency-distribution algorithm:

1. Real part first (`isPendingPhysicalArrival: false`) - guaranteed stock goes to the most urgent orders.
2. Whatever's left in stock after pass 1, plus the phantom part, as pass 2 (`isPendingPhysicalArrival:
   true`).

Real-first is deliberate: give certainty to the most urgent orders, push uncertainty further down the
queue. It was verified that pass 2 can never accidentally mislabel leftover real stock as phantom: pass 1
only leaves stock unassigned when it has run out of *candidates* (every matching `ForSewing` order already
satisfied), and if there are no candidates left, pass 2 - querying the same candidate set - finds none
either, so any leftover just becomes plain `QuantityInStock`, untagged, with no order affected either way.

`UpdateOrderService.UpdateAsync` additionally, after this split, checks
`HasOpenPendingReturnsForOrderAsync(order.Id)` on **the edited order itself** and detaches the tag if
nothing is left open (see "Bugs found and fixed" item 2 - this was originally missing entirely).

`DeleteOrderService.DeleteAsync` does not need an equivalent check - the order is gone.

**Early-exit path in `DeleteOrderService.DeleteAsync`**: orders already in a rejected/received/`Refusal`/
`Exchange` status skip the whole `HandleDeletedOrderItems` flow (their stock effects were already applied
when they transitioned into that status). `Packed` is **not** in that early-exit list, so deleting a
`Packed` order *does* go through the full flow - this is exactly why the `Packed`-transition ledger fix
(item 5 below / "Bugs found and fixed" item 5) matters: without it, deleting a Packed order would find a
stale "still open" ledger row and incorrectly propagate it as phantom to someone else.

### 6. Order manually transitions to `Packed` - physical arrival confirmed

`SetOrderManualStatusService.SetOrderManualStatusAsync`, `Packed` branch
(`Katino.Infrastructure/Persistance/Services/Order/SetOrderManualStatusService.cs`). Only reachable from
`ReadyToShip`. In addition to detaching the `PendingIncomingReturn` tag, it now also resolves the ledger:

```csharp
foreach (var orderItem in order.OrderItems)
{
    var pendingRemaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(orderItem.Id);
    if (pendingRemaining > 0)
        await _redistributionHistoryRepository.ConsumePendingReturnAsync(orderItem.Id, pendingRemaining);
}
```

No new ledger row is written here (nothing needs forwarding - the goods are physically present and are
being packed for exactly this order, consumed in place, not freed up for anyone else). This must run
*before* the tag detach check that gates on `HasOpenPendingReturnsForOrderAsync` elsewhere staying
consistent, though order doesn't strictly matter here since this method unconditionally detaches whenever
the order was tagged.

## Repository API (`IProductVariantRedistributionHistoryRepository`)

`Katino.Domain/Repositories/ProductVariantRedistributionHistoryRepository/IProductVariantRedistributionHistoryRepository.cs`
/
`Katino.Infrastructure/Persistance/Repositories/ProductVariantRedistributionHistoryRepository/ProductVariantRedistributionHistoryRepository.cs`

- `GetByOrderIdAsync(orderId)` - full audit trail for an order (pre-existing, unchanged), used by
  `GET /api/Order/{id}/redistribution-history`.
- `GetPendingIncomingReturnsAsync(sewerId, ct)` - the queue query. Grouped by `TargetOrderItemId`, summed.
  Filters by `IsPendingPhysicalArrival`, open balance, tag presence, and sewer visibility.
- `GetPendingReturnRemainingAsync(orderItemId, ct)` - `SUM(Quantity - QuantityResolved)` across every
  open row for one item. The single formula for "how much is still owed to this item" - every caller that
  needs this number uses this method, so the formula only exists in one place.
- `ConsumePendingReturnAsync(orderItemId, quantity, ct)` - closes up to `quantity` across open rows for
  one item, **oldest first (FIFO by `CreatedAtUtc`)**. Returns how much was actually consumed (may be less
  than requested - it's a mechanical, non-validating operation; callers must validate against
  `GetPendingReturnRemainingAsync` beforehand if they need to reject over-consumption). FIFO isn't a hard
  business requirement (quantity is fungible, not tied to a specific physical shipment) - it's chosen so
  the ledger stays predictable to read: older promises close first, so whatever's still open is always the
  most recent.
- `HasOpenPendingReturnsForOrderAsync(orderId, ct)` - used everywhere a tag-detach decision is made:
  "does this order have *any* remaining open pending coverage, on any of its items?"

This is the one and only place that knows how to compute/consume the pending ledger. **Every** flow above
(sewing a replacement, urgent reallocation, order deletion/edit, Packed confirmation) calls into these same
four methods rather than touching `ProductVariantRedistributionHistory` rows directly - if the closing
semantics ever need to change, there is exactly one place to fix it.

## `IUpdateProductVariantService.HandleProductVariantQuantityChange` - the distribution primitive

`Katino.Infrastructure/Persistance/Services/ProductVariant/UpdateProductVariantService.cs`. Pre-existing
method, used by essentially every quantity-redistribution path in the whole app (`ManualEdit`,
`OrderRejected`, `OrderDeleted`, `OrderEdited`, `Sewing`, and now the `ReturnCoveredBySewing` forward
call). Walks `GetActiveOrdersWithSpecificProductVariantAsync` from most to least urgent, consuming
`newQuantity` (an **absolute** pool size, not a delta) as it goes, until either the pool or the candidates
run out; any leftover becomes the product variant's new `QuantityInStock`.

Two changes for this feature:

1. **`isPendingPhysicalArrival` parameter**, forwarded verbatim into the recorded
   `ProductVariantRedistributionEvent`. **Must always be passed explicitly by the caller - never inferred
   from current DB state inside this method.** By the time `ApplyIncomingReturnSewedAsync` calls this (to
   forward the freed-up still-incoming shipment), the originating pending-return row has *already* been
   consumed by the earlier `ConsumePendingReturnAsync` call in the same transaction - re-deriving "is this
   phantom" from current state at that point would incorrectly see nothing open and default to `false`.
2. **Per-order, all-matching-items loop** instead of `FirstOrDefault`. An order can have more than one
   item of the same product variant (one real-covered, one phantom-covered is the exact motivating case -
   see "Bugs found and fixed" item 1); the old `FirstOrDefault` could silently starve a genuinely-needy
   sibling item while wrongly favoring an already-satisfied one it happened to pick first, or pick the
   already-`Ready` one and skip the whole order via the `OrderItemStatus.Ready` short-circuit even though a
   sibling item still needed quantity.

## `ProductVariantRedistributionEvent` / `ProductVariantRedistributionLine`

`Katino.Domain/Models/ProductVariantRedistributionEvent.cs`. The envelope every write path builds before
calling `ProductVariantRedistributionRecorder.RecordAsync`. `IsPendingPhysicalArrival` lives at the
**event** level (applies to every line in the event uniformly) - if a caller needs to send a mix of real
and phantom lines in one logical operation, it must send two separate events (this is exactly what
`HandleProductVariantQuantityChangeSplit` does).

## Bugs found and fixed while building this (read before touching this code again)

These were all found by deliberately walking through the exact scenario of "one order item real-covered,
a sibling item of the same order phantom-covered" and tracing the numbers through every code path. If a
future change reintroduces any of these, it will most likely surface as: an order shows `Ready`/tag-free
when it shouldn't, a more urgent order fails to get tagged when it takes over a phantom claim, or the
`PendingReturnInvariantHelper` log starts firing.

1. **`FirstOrDefault` picking the wrong sibling item.** Both `HandleProductVariantQuantityChange`'s
   target-item lookup and `UrgentOrderRedistributionService`'s donor-item lookup used to take the first
   matching `OrderItem` in an order, ignoring the possibility of a second matching item with different
   phantom/real backing. Fixed by iterating all matching, non-`Ready` items per order/donor instead of
   just the first.
2. **Tag never detached after `OrderEdited`.** `UpdateOrderService.UpdateAsync` computed the real/phantom
   split correctly but never checked whether the edited order itself still had open pending coverage
   afterward - a stale `PendingIncomingReturn` tag could live forever on a live order that no longer needed
   it. Fixed by adding a `HasOpenPendingReturnsForOrderAsync` check + detach at the end of the update flow.
3. **`ApplyIncomingReturnSewedAsync` almost mutated `QuantityToProduce`.** The first draft assumed the
   order item was always fully phantom-covered (`QuantityToProduce == 0` going in) and that sewing a
   replacement should mark the item fully `Ready`. This breaks the moment the item previously lost part of
   its phantom coverage to urgent reallocation (its `QuantityToProduce` went back up for that stolen
   part) - sewing the *remaining* pending amount does not retroactively cover the *separately* unmet
   portion. Resolved by proving the invariant algebraically: closing a pending promise with real
   production always leaves `QuantityToProduce` unchanged, in every case, not just the simple one. The
   method deliberately never touches `QuantityToProduce`/`OrderItemStatus`.
4. **Full ledger closure on partial order edits.** `ProcessOrderItemProductVariantDeletion`, used for both
   real deletions and the "old half" of an item update, used to always release the item's *entire* open
   pending balance, even when the item's `OrderItem.Id` survives the edit with a reduced (but still
   non-zero) quantity. Concretely: item had 1 real + 2 phantom units (`Quantity=3`), customer reduces to
   `Quantity=2` (a reduction of only 1) - the correct outcome is 1 phantom unit released, 1 phantom unit
   *retained* on the same item. The old code released both phantom units and never gave the retained one a
   new home anywhere - it silently vanished from tracking, and the edited item ended up looking fully
   `Ready` with zero backing for part of its "coverage" (worse than before: not even tracked as pending
   anymore). Fixed with the `survivingQuantity` parameter - originally as `min(freedQuantity, reduction,
   phantomRemaining)` where `reduction = oldQuantity - survivingQuantity` - see item 6 for why that
   formula itself still wasn't quite right.
5. **`Packed` transition only cleared the tag, not the ledger.** The underlying `Quantity >
   QuantityResolved` rows stayed open forever once an order was packed - invisible in the queue (tag gone)
   but still "open" to any later `GetPendingReturnRemainingAsync`/`ConsumePendingReturnAsync` call. If that
   Packed order was later deleted or edited, the stale open amount would be treated as still-in-transit
   and incorrectly propagated (with a tag) to some other order, even though the goods had already
   physically arrived and shipped. Fixed by resolving every order item's open pending balance at the
   moment of the `Packed` transition, not just detaching the tag.
6. **`reduction` measured against raw `Quantity`, not against what was actually covered.** The item-4 fix
   above used `reduction = oldQuantity - survivingQuantity` to cap how much pending coverage gets
   released on an edit. This is wrong whenever the item already had a genuinely-uncovered portion
   (`QuantityToProduce > 0`) before the edit: increase quantity 1→3 (the 1 unit stays fully phantom,
   `QuantityToProduce` correctly becomes 2 - see item 4's mechanism), then decrease 3→1. `freedQuantity`
   at that point is only 1 (`Quantity(3) - QuantityToProduce(2)`), but the old formula computed
   `reduction = 3 - 1 = 2`, capped only by `phantomRemaining(1)` - so it released the *entire* remaining
   phantom unit, even though the surviving quantity (1) was fully absorbed by shrinking the
   already-uncovered part (2 → 0) and never should have touched the covered part at all. Symptom: the tag
   and `ForSewing` status both incorrectly disappear after an increase-then-decrease-back-to-original
   round trip. Fixed by capping the release against `freedQuantity` (what was actually covered) instead of
   the raw `Quantity` delta: `coverageReduction = Math.Max(0, freedQuantity - survivingQuantity)`,
   `phantomPart = Math.Min(coverageReduction, phantomRemaining)`. Only release coverage for the part of
   the reduction that eats into what was covered; a reduction that's fully absorbed by the
   already-uncovered portion releases nothing.

## Known non-goals / things intentionally not handled

- Concurrency: no explicit locking around ledger reads/consumes. Same characteristic as the rest of this
  codebase (no optimistic concurrency tokens on these tables); not something this feature introduced or
  fixed.
- The standalone `IncomingReturnQueueService` / `GET /api/OrderItem/incoming-return-queue` endpoint is kept
  for potential future use but not wired into any other flow - it duplicates `GetPendingIncomingReturnsAsync`
  logic on its own (joins `OrderItemRepository.GetByIdsAsync` separately) rather than reusing
  `SewingQueueService`'s merged path. If you change the pending-return query shape, check this file too.
- `UrgentOrderRedistributionService`'s global "phantom across all donors before real from any donor"
  ordering is a deliberate choice (see flow 4 above) but does mean donor priority order can be
  overridden by phantom-availability. If this ever needs to change back to strict per-donor priority, revisit
  the two-pass (`foreach (var takeFromPhantom in new[] { true, false })`) structure.

## File map

| File | Role |
|---|---|
| `Katino.Domain/Entities/ProductVariantRedistributionHistory.cs` | Ledger entity - `IsPendingPhysicalArrival`, `QuantityResolved` |
| `Katino.Domain/Enums/ProductVariantQuantityChangeReason.cs` | `Reason` enum incl. `ReturnCoveredBySewing` |
| `Katino.Domain/Models/SewingQueueItem.cs` | Unified queue row - `IsIncomingReturn`, `SendUntil` |
| `Katino.Domain/Models/PendingIncomingReturnSummary.cs` | Aggregated queue projection |
| `Katino.Domain/Models/ProductVariantRedistributionEvent.cs` | Write envelope - `IsPendingPhysicalArrival` |
| `Katino.Domain/Helpers/SewingQueueGroupingHelper.cs` | Queue grouping rules |
| `Katino.Domain/Helpers/PendingReturnInvariantHelper.cs` | Safety-net invariant check |
| `Katino.Domain/Repositories/ProductVariantRedistributionHistoryRepository/IProductVariantRedistributionHistoryRepository.cs` | Repository contract |
| `Katino.Infrastructure/Persistance/Repositories/ProductVariantRedistributionHistoryRepository/ProductVariantRedistributionHistoryRepository.cs` | Repository implementation - the ledger math |
| `Katino.Infrastructure/Persistance/Context/KatinoDbContext.cs` | Column config for the new/renamed fields |
| `Katino.Infrastructure/Persistance/Services/ProductVariantRedistribution/ProductVariantRedistributionRecorder.cs` | Single write+tag point |
| `Katino.Infrastructure/Persistance/Services/ProductVariant/UpdateProductVariantService.cs` | `HandleProductVariantQuantityChange` (multi-item fix, `isPendingPhysicalArrival`), `HandleProductVariantQuantityChangeSplit` |
| `Katino.Infrastructure/Persistance/Services/Order/UrgentOrderRedistributionService.cs` | Donor selection, phantom-first, multi-item fix |
| `Katino.Infrastructure/Persistance/Services/Order/DeleteOrderService.cs` | `HandleOrderRejectionAsync` (unsplit), `DeleteAsync` (split) |
| `Katino.Infrastructure/Persistance/Services/Order/UpdateOrderService.cs` | Split + tag cleanup on edit |
| `Katino.Infrastructure/Persistance/Services/Order/SetOrderManualStatusService.cs` | `Packed` resolves the ledger |
| `Katino.Infrastructure/Persistance/Services/OrderItem/OrderItemChangeService.cs` | `phantomQuantitiesFreed` computation, `survivingQuantity` |
| `Katino.Infrastructure/Persistance/Services/OrderItem/SewingProductionReportService.cs` | Three-way sewing report branch, `ApplyIncomingReturnSewedAsync` |
| `Katino.Infrastructure/Persistance/Services/OrderItem/SewingQueueService.cs` | Merged queue |
| `Katino.Infrastructure/Persistance/Services/ProductVariantRedistribution/IncomingReturnQueueService.cs` | Standalone queue (unused elsewhere) |
| `Katino.Application/DTOs/OrderItem/SewingQueueItemDto.cs` | `IsIncomingReturn`, `SendUntil` on the wire |
| `Katino.Application/DTOs/ProductVariantRedistributionHistory/ProductVariantQuantityChangeReasonDto.cs` | Mirrors the domain enum |
| `Katino.Web/Controllers/OrderItemController.cs` | `GET /sewing-queue`, `/sewing-queue-grouped`, `/incoming-return-queue`, `POST /sewing-report` |
