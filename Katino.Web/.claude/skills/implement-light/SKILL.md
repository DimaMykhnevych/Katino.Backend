---
name: implement-light
description: Lightweight architect -> developer -> tester pipeline for a feature/bugfix task in this repo, with explicit user confirmation before implementation and before any migration step, but no reviewer/security-reviewer stages and no persisted/resumable run state. Use for small, low-risk changes where a full /implement review pass is unnecessary overhead.
argument-hint: <task description>
allowed-tools: Agent
model: inherit
---

You are orchestrating a lightweight, three-stage implementation pipeline for this repo. This is a
production CRM handling real orders and money plus a customer-facing storefront — **never skip the
confirmation gates below**, even under time pressure or if the task sounds trivial. The only difference
from `/implement` is that this skill stops after `tester` (no `reviewer`/`security-reviewer` stages) and
does not persist run state to disk — it lives entirely in this conversation, so it cannot be paused and
resumed across sessions the way `/implement` can. If the user wants either of those, point them at
`/implement` instead of trying to replicate it here.

## Phase: architect
Spawn the `architect` subagent (Agent tool, `subagent_type: architect`) with the task description:

$ARGUMENTS

Print its plan to the user in full. If it says a migration is needed, call that out prominently and
separately from the general plan approval — a migration means the repo owner generates/applies it
manually later, not this pipeline.

**Stop your turn here and wait for the user's explicit confirmation.** Do not proceed silently.

## Phase: developer
(Entered only after plan confirmation.)

If the plan flagged a migration: explain what it would need to change and **stop your turn and wait for a
separate explicit confirmation** before continuing — this is a second gate, distinct from the plan
approval, per this repo's rule that migrations always get their own confirmation. (The `developer` agent
itself also refuses to run `dotnet ef`/touch `Migrations/` regardless — this gate is about proceeding with
the rest of the implementation with the awareness a migration is still pending, not about permission to
run the migration command, which never happens in this pipeline.)

Spawn the `developer` subagent (Agent tool, `subagent_type: developer`) with the confirmed plan. Print its
summary (files changed, build result, migration-pending flag if any) to the user.

## Phase: tester
Spawn the `tester` subagent (Agent tool, `subagent_type: tester`) with the developer's change summary —
not the full diff, keep the prompt small. Print its result (what was tested, how, pass/fail, and the
test-project-gap note if it applies) to the user.

## Finish
Give the user a short summary: what was built, test result, and whether a migration is still pending
manual generation/application. Mention explicitly that this run skipped code-review and security-review —
if the change touches anything auth/data-sensitive, suggest `/review` and `/security-review` as a
follow-up rather than assuming they're covered.
