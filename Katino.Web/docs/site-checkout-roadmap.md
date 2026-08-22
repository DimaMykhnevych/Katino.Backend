# Storefront Checkout Roadmap (Cart, Customer Accounts, Payment, Orders)

## Status

**Planning.** Nothing described here is implemented yet. This document is the source of truth for the
plan agreed with the repo owner across several conversations; work will happen phase by phase, likely in
separate chats/sessions. If you are an agent picking up one phase, read this whole document first (it's
short per phase), then re-verify the "current state" claims below against the actual code before writing
anything — they were accurate as of 2026-08-09 but earlier phases may have changed things since.

As with other docs in this folder, EF Core migrations are applied manually by the repo owner — **check
with the user before assuming a migration exists or has been applied**, and before generating one.

## Why this exists

The storefront (`site/Katino.Store.Web` + `site/Katino.Store.Application`) already has catalog browsing,
product detail, search and filtering working against the shared `KatinoDbContext` (same database as the
CRM app, `Katino.Web`). To actually launch the site, customers need to be able to add products to a cart
and pay for an order, and that order needs to land in the existing CRM order-fulfillment system so staff
can process it exactly like any other order.

The CRM's `Order` model and fulfillment pipeline (Nova Poshta logistics, stock decrement, finance ledger,
tags) already exist and are mature — the goal is to feed it from the storefront, not rebuild it.

## Ground truth about the existing system (read before touching anything)

- **`Order` (`Katino.Domain/Entities/Order.cs`)** is Nova-Poshta-centric: every order needs a sender NP
  warehouse/city/contact person, delivery type, weight, seats amount, etc. A site checkout DTO must default
  or derive all of this rather than ask the customer for it — the customer only supplies what's genuinely
  theirs to supply (delivery preference/address, contact info, cart contents).
- **`AddOrderService.AddAsync`** (`Katino.Infrastructure/Persistance/Services/Order/AddOrderService.cs`,
  wired as `IAddOrderService`) is the one existing place that creates an order end-to-end: upserts NP
  city/contact person, resolves delivery/recipient via `IOrderDeliveryHandlerFactory`, optionally
  recalculates pricing, computes item statuses via `IOrderItemChangeService`, inserts the order + a
  `FinanceEntry` revenue row, attaches tags, decrements stock (`HandleAddedOrderItems`), and (outside the
  transaction) creates the NP shipping label. Site checkout should reuse this service (or a thin sibling
  that still funnels through it) rather than reimplementing order insertion — read it fully before Phase 3.
- **Stock**: the actual available-to-sell quantity is simply `ProductVariant.QuantityInStock` — it is
  maintained directly (staff edits it via `UpdateProductVariantService`, which also adjusts it internally
  during urgent-order redistribution). `QuantityDropSold`/`QuantityRegularSold` are cumulative sold
  counters incremented by `OrderItemChangeService.HandleAddedOrderItems` at order-creation time — they are
  **not** subtracted from `QuantityInStock` to get availability; `TotalSold` can exceed `QuantityInStock`
  over time, which is exactly why the old computed property `AvailableQuantity =>
  QuantityInStock - TotalSold` (line 35 of `ProductVariant.cs`) was wrong and is being removed from the
  entity (it wasn't referenced anywhere in the codebase). **Use `QuantityInStock` directly wherever
  "available quantity" is needed** — including for the storefront. There is currently **no concept of a
  temporary hold** (add-to-cart doesn't touch stock at all anywhere in the codebase) — that's new, see
  Phase 2.
- **Tags**: `OrderTagType` (`Katino.Domain/Enums/OrderTagType.cs`) is a fixed enum (`NotNpOrder`,
  `RefundMoney`, `Custom`, `PendingIncomingReturn`), attached via `OrderTagRepository.GetOrCreateByTypeAsync`
  + `AttachTagToOrderAsync`. A "came from the site" marker should be a new enum member here (see Phase 3),
  following the same pattern as `NotNpOrder`.
- **Identity**: the storefront currently authenticates against `AppUser` (`Katino.Domain/Entities/
  AppUser.cs`), the *same* Identity table used by CRM staff logins — there is no registration endpoint on
  the site at all yet, only `POST /api/Auth/token` (login). We've decided this is wrong for customers long
  term (see Phase 1 rationale) and are building a separate `Customer` identity instead of extending
  `AppUser`.
- **No existing infrastructure for**: cart/basket (nothing), payment gateway integration (nothing),
  invoices/receipts/fiscal checks (nothing — not even stubs).
- **Recurring background jobs**: there's a working precedent — `Katino.Functions` is an Azure Functions
  worker project with a real timer-triggered job today, `Katino.Functions/Functions/NpStatusSync.cs`
  (`[Function("RunNpStatusSync")]`, `[TimerTrigger("%NpStatusSyncTimerSchedule%")]`, schedule from config,
  delegates to an injected service — `INpIntDocStatusSyncService`). A cart-expiry sweeper should follow this
  exact shape (new `[Function]` + `[TimerTrigger]` + its own injected service) rather than inventing a
  `BackgroundService` in `Katino.Store.Web` (the one `BackgroundService` that exists there today,
  `NovaPoshtaSyncBackgroundService`, is a one-shot startup task in `Katino.Web`, not a periodic-job
  precedent — `Katino.Functions` is the right home for this).
- Both apps share one `KatinoDbContext`/database — no schema separation between "site" and "CRM" data.

## Decisions already made (don't re-litigate without a reason)

1. **Separate `Customer` table**, not a reuse of `AppUser`. Rationale: `AppUser` is shared with CRM staff
   auth; mixing customer PII/volume and customer-only fields (loyalty, marketing consent, saved addresses)
   into that table was judged not worth the coupling once we're building it properly, even though it costs
   more upfront (own registration/login/JWT claims — see Phase 1).
2. **Both guest and registered checkout** must work through the same checkout endpoint(s) — optional auth,
   not two separate flows. If a valid customer JWT is present, the order links to `CustomerId`; if not, the
   order is anonymous and carries contact info directly (mirroring how `OrderRecipient`/`OrderAddressInfo`
   already store contact info separately from any user account).
3. **Site orders reuse the existing `Order` entity/pipeline**, tagged with a new `OrderTagType` (working
   name: `FromSite`) so CRM staff can immediately see which orders came from the storefront. No new
   `SaleType` — site orders use `SaleType.Retail` like any other retail sale; the tag is the identifying
   signal, not the sale type.
4. **The checkout DTO from the frontend carries only what the customer actually decides** (cart reference,
   delivery choice/address, contact info, payment method). Everything else the current `Order`/
   `AddOrderCommand` needs (sender warehouse/city/contact person, pricing, weight/seats, description) is
   filled in server-side from config/derivation — never trust/accept these from the client. Pricing in
   particular must always be recomputed server-side from current DB prices, never taken from the cart
   payload as-is.
5. **Cart reservations expire on a timeout.** While an item sits in a cart (added but not yet paid/
   confirmed), it counts against availability shown to *other* shoppers, but releases automatically if the
   timeout passes without the order being confirmed. This needs a new ledger; availability is `QuantityInStock - SUM(active reservations)` (see "Ground truth" above — not the old `AvailableQuantity` formula). The
   reservation ledger is separate from `QuantityRegularSold`/`QuantityDropSold`, which are just sold
   counters incremented at real order confirmation (via `OrderItemChangeService`) and don't themselves
   drive availability.
6. **Payment gateway is still an open research question** — the constraint is avoiding a revenue-share/
   percentage cut where possible (money should land directly in the FOP's bank account), and fiscal receipts
   (checks) to customers are a legal requirement, not optional. This needs explicit research before picking
   a provider (see Phase 4) — don't assume a 0%-fee processor exists in Ukraine without checking; that may
   turn out to be an unrealistic constraint and need revisiting with the user.
7. **Invoices/payment records get stored in the DB**, not just referenced from a third-party dashboard —
   design an `Invoice`/`PaymentTransaction` entity (Phase 5), using the existing `FinanceEntry` ledger
   (`Katino.Domain/Entities/FinanceEntry.cs`) as a structural reference point since it already records
   order revenue and a payment table will need to interoperate with it.

## Phases

Each phase below is meant to be workable as its own chat session. Read "Ground truth" and "Decisions"
above first, then just the phase you're doing.

### Phase 1 — Customer identity (separate table) — **Implemented, migration pending**

Code is in place: `Customer` entity, `ICustomerRepository`/`CustomerRepository`, `ICustomerAuthorizationService`/
`CustomerAuthorizationService` (standalone, does not inherit `BaseAuthorizationService`), `RegisterCustomer`/
`ConfirmCustomerEmail`/`SignInCustomer` commands, `CustomerAuthController` (`POST /api/CustomerAuth/register`,
`/confirm-email`, `/token`). Customer JWTs carry `aud = AuthOptions.CUSTOMER_AUDIENCE` and
`ClaimTypes.Role = Role.Customer`; `site/Katino.Store.Web`'s `IdentityInstaller` accepts both audiences,
`Katino.Web` (CRM) still only accepts the staff one. Email confirmation is mandatory (mirrors the CRM
`AppUser` flow) via a random token stored on `Customer` (24h expiry) — no `UserManager<Customer>` involved.
**The EF Core migration for the new `Customers` table has not been generated/applied** — the repo owner
does this manually (see Status note above). Full file list and rationale: see the plan file this was
implemented from, or the commit(s) that introduced it.

**Goal**: customers can register and log in on the storefront, independently of CRM `AppUser` accounts.

- New `Customer` entity (own table) — email, phone, password hash, registration date, marketing-consent
  flag, etc. Since ASP.NET Identity's `AddIdentity<TUser,TRole>()` is designed for one user type per
  context/registration, decide explicitly whether to: (a) hand-roll auth for `Customer` using
  `PasswordHasher<Customer>` standalone (no full Identity), or (b) find another way to run two `IdentityUser`
  types against the same `DbContext`. Recommendation from prior discussion: (a), it's simpler and this
  isn't CRM-staff-grade auth.
- `POST /api/Auth/register` (new) and adapt/parallel the existing `POST /api/Auth/token` login for
  `Customer` (currently that endpoint only knows about `AppUser` via `AppUserAuthorizationService`/
  `BaseAuthorizationService`).
- JWT claims for a customer token need to be clearly distinguishable from a CRM staff token. Recommended:
  give the two apps distinct `ValidAudience` values (currently shared via `Katino.Domain.Auth.AuthOptions`)
  so a customer token can never pass validation against the CRM API even in principle, rather than relying
  solely on role checks.
- Password reset / email confirmation flow — decide scope (may be deferred past initial launch).
- Not in scope for this phase: cart, checkout, payment. Just accounts.

### Phase 2 — Cart and stock reservation

**Goal**: add-to-cart works for both guest and registered customers, and reduces *displayed* availability
for everyone else, with automatic release on timeout.

- New `Cart`/`CartItem` entities (guest carts keyed by an anonymous session/device id — decide mechanism,
  e.g. a cart token stored client-side; registered customers' carts keyed by `CustomerId`).
- New reservation ledger (separate from `ProductVariant.QuantityRegularSold`/`QuantityDropSold`, which are
  just cumulative sold counters, not stock) — e.g. a `StockReservation` row per cart item with `ExpiresAt`.
  Adding to cart creates/refreshes a reservation; removing from cart or letting it expire deletes/releases
  it.
- Availability shown to shoppers must be `QuantityInStock - SUM(active reservations across all carts,
  including the viewer's own)` — compute this live in the product/cart queries, don't rely solely on a
  cleanup job to keep it correct (avoid a race window). Do **not** resurrect the old `AvailableQuantity`
  formula (`QuantityInStock - QuantityDropSold - QuantityRegularSold`) — that property is being removed
  from `ProductVariant` because `TotalSold` can exceed `QuantityInStock` and drive it negative; it was
  unused anywhere in the codebase.
- Concurrency: two customers adding the last unit "at the same time" must not both succeed — reserving
  needs a transaction with a concurrency-safe check (the existing stock-mutation code in
  `OrderItemChangeService`/`UpdateProductVariantService` doesn't have to solve this today because order
  creation is comparatively rare/sequential in the CRM; carts will see far more concurrent writes, so don't
  just copy that pattern uncritically).
- New recurring job to sweep/delete expired reservations for table hygiene — add it to `Katino.Functions`
  following the existing `NpStatusSync.cs` shape (new `[Function]` method + `[TimerTrigger("%SomeConfig
  Key%")]` + an injected service doing the actual work), not as a new `BackgroundService` in
  `Katino.Store.Web`. This job is for hygiene only, not the source of truth for availability (see above —
  availability is computed live).
- Decide the timeout duration (needs a product decision, not just an engineering one).

### Phase 3 — Checkout / Order creation from the site

**Goal**: a confirmed cart becomes a real `Order`, visible and processable in the CRM exactly like any
other order.

- Design the minimal `SiteCheckoutRequestDto`: cart reference, delivery choice (NP warehouse pickup vs
  address delivery — reuses `OrderRecipient`/`OrderAddressInfo` shapes), contact info (name/phone/email —
  required for guests, prefillable from `Customer` for registered users), payment method choice, optional
  comment. Explicitly *not* included: price/cost (always recomputed server-side), sender warehouse/city/
  contact person (shop's own origin — needs a config value, since today `AddOrderCommand` requires the
  caller to supply these; check whether such a "default sender" config already exists in the CRM before
  inventing a new one), weight/seats (derive or default), `Description`/`GeneralOrderInfo` (auto-generate).
- New `OrderTagType.FromSite` (or similar name — confirm with user), attached the same way `NotNpOrder` is
  today (`GetOrCreateByTypeAsync` + `AttachTagToOrderAsync`, `canBeDeleted: false`).
- Reuse `IAddOrderService.AddAsync` (or a thin storefront-specific wrapper around it) so stock decrement,
  finance ledger entry, item status computation, and NP label creation all stay centralized — resist the
  urge to duplicate that logic for "just the site."
- On successful order creation, the cart's `StockReservation` rows for the checked-out items convert into
  the real stock decrement done by `AddOrderService` — make sure the reservation is released/consumed
  exactly once here (not double-counted, not left dangling).
- Link `Order` to `CustomerId` when authenticated, leave null for guests (contact fields carry the
  guest's info instead, same as `OrderRecipient` already does for CRM-entered orders today).

### Phase 4 — Payment gateway research and integration

**Goal**: customers can actually pay online, money reaches the FOP's account, order gets marked paid.

- Research phase first, don't default into an implementation. Constraints to satisfy: avoid a
  percentage-of-revenue cut if at all achievable (sanity-check this against reality — Ukrainian acquiring
  almost universally charges a %, e.g. LiqPay/Fondy/WayForPay/Monopay; a truly 0%-fee automated option may
  not exist, and manual IBAN transfer trades the fee for automation/UX/reconciliation cost — surface this
  tradeoff to the user rather than silently picking one), fiscal receipt issuance is a hard legal
  requirement, not optional, for a Ukrainian FOP.
- Whatever provider is chosen, design the integration payment-provider-agnostic where reasonable (a
  `Payment`/transaction abstraction, provider-specific webhook handler translating into it) so switching
  providers later isn't a rewrite.
- Order status needs a "paid"/"awaiting payment" signal — check whether existing `OrderStatus`
  (`Katino.Domain/Enums/OrderStatus.cs`) already has something usable or needs extending.

### Phase 5 — Invoices / fiscal receipts

**Goal**: every paid order has a persisted invoice/receipt record, and the customer receives their legally
required fiscal check.

- New `Invoice`/`PaymentTransaction` entity — use `FinanceEntry` (`Katino.Domain/Entities/FinanceEntry.cs`)
  as a structural reference since it already models order-linked money movement (`EntryDate`, `Amount`,
  `SourceType`, `Reason`, `IsLocked`, `OrderId`) — decide whether invoices reference/extend it or sit
  alongside it feeding into it.
- Fiscalization: research whether the payment gateway chosen in Phase 4 can issue the fiscal check directly
  (some Ukrainian acquirers bundle this) or whether a separate RRO/PPO provider (e.g. checkbox.ua,
  vchasno.kasa) integration is needed. This is a legal-compliance area — verify actual requirements with the
  user, don't assume.
- Store the receipt reference/PDF-or-link so it can be re-sent to the customer later (order history,
  support requests).

### Phase 6 — CRM-side visibility (may fold into earlier phases if small enough)

**Goal**: staff can see storefront customers and their orders/payments from the CRM without extra tooling.

- A read view/controller in `Katino.Web` for `Customer` records (the `KatinoDbContext` is already shared,
  so this is additive, not a new integration).
- Surface the `FromSite` tag and payment/invoice status in the existing order views.

## Open questions to resolve before/during the relevant phase

- Exact cart-reservation timeout duration (Phase 2) — product decision.
- Where does the shop's own Nova Poshta sender identity config live, if anywhere yet, for Phase 3 to reuse?
- Final `OrderTagType` name for site orders (`FromSite` used as a placeholder above).
- Whether guest checkout requires email confirmation of the order (fraud/typo protection) before it's
  processed, or ships as soon as payment clears.
- Payment provider choice and whether the 0%-fee constraint survives contact with reality (Phase 4).
- Fiscalization provider/approach (Phase 5).
