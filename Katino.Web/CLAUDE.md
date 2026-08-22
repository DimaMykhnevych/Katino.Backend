# Katino.Backend — conventions for Claude Code

This is a .NET monorepo for a real, in-use CRM (`Katino.Web`) plus a customer-facing storefront
(`site/Katino.Store.Web` + `site/Katino.Store.Application`), sharing one `KatinoDbContext`/database via
`Katino.Domain`/`Katino.Application`/`Katino.Infrastructure`. `Katino.Functions` is an Azure Functions
worker for periodic jobs. Treat changes here as production-affecting — orders, stock, and money are real.

This file covers only what isn't obvious from reading the code.

## CQRS / MediatR

Commands and queries live under `Commands/<Entity>/<Action>/<Action>Command.cs` +
`<Action>CommandHandler.cs` (same shape for `Queries/`), one pair per action, implementing MediatR's
`IRequest<T>` / `IRequestHandler<TCommand, T>`. Request validation is plain DataAnnotations attributes on
the Command/Query class (see `RegisterCustomerCommand.cs`) — **this repo does not use FluentValidation**;
don't introduce it without a stated reason.

`Katino.Web` (CRM) and `site/Katino.Store.Application` (storefront) have their own, separate
Commands/Queries even though they share the underlying domain/infrastructure — a feature almost never
belongs in both.

## Dependency injection — the Installer pattern

Each app (`Katino.Web`, `site/Katino.Store.Web`) has an `Installers/` folder with one `IInstaller`
implementation per concern (`DbInstaller`, `IdentityInstaller`, `RateLimitingInstaller`,
`ServiceComponentsDiInstaller` for repository/service registrations, etc.), all wired up via
`builder.Services.InstallServices(builder.Configuration)` in `Program.cs`. New services/repositories get
registered in the relevant `Installers/*.cs`, not registered ad hoc in `Program.cs`.

## EF Core migrations — manual only, always confirm first

**Migrations in this repo are generated and applied manually by the repo owner, never automatically by an
agent.** Never assume a migration exists or has been applied for a data-model change you're implementing.
If a task implies one, implement the entity/code side of it and say clearly that the migration itself
still needs to be generated (`dotnet ef migrations add ...`) and applied by hand — and get explicit
confirmation from the user before treating any migration step as something to run, even if the rest of the
task was already approved. This applies to every agent/skill in this repo, not just `/implement`'s gate.

## `docs/` — persistent architecture memory

`docs/*.md` holds design docs for nontrivial, cross-session-relevant decisions (existing examples:
`docs/incoming-return-tracking.md`, `docs/site-checkout-roadmap.md`). Format: **Status → Why →
Vocabulary/Ground truth → Data model → Phases**. Read the relevant doc before designing a change in an area
it covers — its "ground truth" and "decisions already made" sections are binding unless you flag a reason
to revisit them. Only add a new doc when a decision is genuinely nontrivial and will matter to a future
agent/session — not for routine changes; most changes need no new doc at all.

## Token-economy policy for the agent pipeline

- The `architect` agent returns a compact plan (bullet list of files + one-line reasons), not an essay —
  downstream agents shouldn't have to parse prose to extract the file list.
- Wide/exploratory code search ("where else is X used", "find something similar to Y") goes through the
  built-in `Explore` agent type rather than reading many files directly into a larger agent's context.
- `/build` and `/test` results get passed forward between pipeline stages (e.g. the reviewer stage reuses
  the tester stage's pass/fail result instead of re-running `dotnet test` itself) rather than re-run by
  every downstream agent unless something changed.
- New `docs/` files are for durable cross-session decisions only, per above.

## The agent pipeline

Five subagents in `.claude/agents/`: `architect` (opus, read-only, plans) → `developer` (sonnet, full
edit) → `tester` (sonnet, writes/runs tests or does manual E2E via `/run` when no test project fits) →
`reviewer` (sonnet, read-only, repo-convention findings on top of the built-in `code-review` skill) →
`security-reviewer` (opus, read-only, repo-specific security findings — JWT audience boundaries, rate
limiting — on top of the built-in `security-review` skill).

- `/implement <task>` runs the full pipeline with confirmation gates before implementation and before any
  migration step. It persists resumable state + a per-phase markdown report under
  `.claude/implement-runs/<run-id>/` — use `/implement resume [run-id]` to pick a paused run back up
  (even in a new session) and `/implement list` to see all runs.
- `/architect`, `/develop`, `/test`, `/review`, `/security-review` run one stage standalone, without the
  pipeline's confirmation gates — use these for a single-stage pass when you don't need the full flow.
- `/build [crm|site|all]` builds and reports a summary with NuGet vulnerability-advisory noise
  (`NU1903`/`NU1902`/`NU1510`) filtered out.
- `/run <crm|site>` boots the CRM or storefront app locally and returns a ready-to-use base URL for
  curl/Swagger checks against a real running instance.

There is currently no automated test project anywhere in the solution — the `tester` agent and `/test`
skill account for this explicitly; see `.claude/agents/tester.md`.

## JWT audiences

`Katino.Domain/Auth/AuthOptions.cs`: `AUDIENCE` (`"Katino.App.User"`) is CRM staff, `CUSTOMER_AUDIENCE`
(`"Katino.App.Customer"`) is storefront customers. CRM's `IdentityInstaller` validates only `AUDIENCE`; the
storefront's validates both. Keep this boundary in mind for any new endpoint's `[Authorize]` placement.
