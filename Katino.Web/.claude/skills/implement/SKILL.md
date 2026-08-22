---
name: implement
description: Full architect -> developer -> tester -> reviewer -> security-reviewer pipeline for a feature/bugfix task in this repo, with explicit user confirmation before implementation and before any migration step. Supports pausing and resuming across sessions. Use for any non-trivial change to this production CRM/storefront.
argument-hint: <task description> | resume [run-id] | list
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, Agent, Skill
model: inherit
---

You are orchestrating the full implementation pipeline for this repo. This is a production CRM handling
real orders and money plus a customer-facing storefront — **never skip the confirmation gates below**,
even under time pressure or if the task sounds trivial.

All run state lives on disk under `.claude/implement-runs/<run-id>/` so the pipeline can be paused (session
ends, user steps away) and resumed later, including in a different session — never rely on this
conversation's memory alone to know what phase you're in once you're past step 1.

## Parsing `$ARGUMENTS`

- `list` (exactly) → list every folder under `.claude/implement-runs/`, read each `state.json`, print a
  table of run-id / task / phase / status. Stop here, do nothing else.
- `resume` or `resume <run-id>` → **resume flow**, see below.
- anything else → **new run**, treat the full argument string as the task description.

## Run state files — `.claude/implement-runs/<run-id>/`

- `run-id` — `<YYYY-MM-DD>-<short-kebab-slug-of-the-task>`, e.g. `2026-08-22-password-min-length`.
- `state.json`:
  ```json
  {
    "taskDescription": "...",
    "createdAt": "ISO-8601",
    "updatedAt": "ISO-8601",
    "phase": "architect | developer | tester | reviewer | security-reviewer | done",
    "status": "in-progress | awaiting-plan-confirmation | awaiting-migration-confirmation | done"
  }
  ```
- One markdown file per completed phase, **named exactly after the phase**, written immediately when that
  phase finishes and never rewritten afterward except `summary.md`:
  `architect.md`, `developer.md`, `tester.md`, `reviewer.md`, `security-reviewer.md`, `summary.md`.
  Each file states what was actually done in that phase — this is the human-readable record, `state.json`
  is just the control-flow pointer.

Update `state.json`'s `updatedAt` (and `phase`/`status` as relevant) every time you touch it.

## New run

1. Derive `run-id`, create `.claude/implement-runs/<run-id>/`, write initial `state.json`
   (`phase: architect`, `status: in-progress`).
2. Go to **Phase: architect** below.

## Resume flow

1. If `<run-id>` given, use that folder. Otherwise pick the most recently `updatedAt` run folder whose
   `status` isn't `done`.
2. If no matching run exists, say so and stop (suggest `/implement list` or starting a new run).
3. Read `state.json` and the most recently written phase `.md` file for context. Tell the user which run
   you're resuming, its task description, and what phase/status it's at.
4. Jump to the matching phase below:
   - `status: awaiting-plan-confirmation` → re-show the plan from `architect.md`, wait for confirmation,
     then continue from **Phase: developer**.
   - `status: awaiting-migration-confirmation` → re-explain the pending migration, wait for confirmation,
     then continue **Phase: developer** from where it left off.
   - `status: in-progress` with `phase: X` → that phase didn't finish writing its `.md` before the
     interruption; re-run **Phase: X** from scratch (its work is idempotent enough to redo — e.g.
     re-spawning `developer` on the same plan should converge to the same result, not duplicate work,
     since it's editing named files, not appending).
   - `phase: done` / `status: done` → nothing to resume, just show `summary.md`.

## Pipeline phases

### Phase: architect
Spawn the `architect` subagent (Agent tool, `subagent_type: architect`) with the task description. Write
its plan to `architect.md`. Set `status: awaiting-plan-confirmation`, save `state.json`.

**Print the plan to the user.** If it says a migration is needed, call that out prominently and clearly
separately from the general plan approval — the user needs to understand a migration means the repo owner
generates/applies it manually later, not this pipeline.

**Stop your turn here and wait for the user's explicit confirmation.** Do not proceed silently.

### Phase: developer
(Entered only after plan confirmation, fresh or resumed.)

Set `phase: developer, status: in-progress`, save `state.json`.

If the plan flagged a migration: **before spawning developer**, set
`status: awaiting-migration-confirmation`, save `state.json`, explain what the migration would need to
change, and **stop your turn and wait for a separate explicit confirmation** — this is deliberately a
second gate, distinct from the plan approval in the architect phase, per this repo's rule that migrations
always get their own confirmation. On confirmation, set `status: in-progress` again and continue. (The
`developer` agent itself also refuses to run `dotnet ef`/touch `Migrations/` regardless — this gate is
about proceeding with the rest of the implementation with the awareness a migration is still pending, not
about permission to run the migration command, which never happens in this pipeline.)

Spawn the `developer` subagent with the confirmed plan (from `architect.md`). Write its summary to
`developer.md`. Set `phase: tester`, save `state.json`.

### Phase: tester
Spawn the `tester` subagent with the developer's change summary (from `developer.md` — not the full diff,
keep the prompt small). Write its result to `tester.md`. Set `phase: reviewer`, save `state.json`.

### Phase: reviewer
Run the built-in `code-review` skill (Skill tool, `skill: code-review`) on the current diff. Then spawn the
`reviewer` subagent with the diff plus `architect.md`'s plan, and pass along `tester.md`'s pass/fail result
instead of telling it to re-run `dotnet test` itself. Write the combined findings to `reviewer.md`. Set
`phase: security-reviewer`, save `state.json`.

### Phase: security-reviewer
Run the built-in `security-review` skill (Skill tool, `skill: security-review`) on the current diff. Then
spawn the `security-reviewer` subagent with the same diff/plan context. Write the combined findings to
`security-reviewer.md`. Set `phase: done, status: done`, save `state.json`.

### Finish
Write `summary.md`: what was built, test result, review findings (and whether they were left for the user
or need action), and whether a migration is still pending manual generation/application. Print the same
summary to the user.

## Notes

- Checkpoints are per-phase, not mid-phase. An interruption while a phase is actively running (as opposed
  to at a confirmation gate) means that phase gets redone on resume — not that any file edits already made
  by `developer` are lost, since those are already on disk regardless of pipeline state.
- Keep each subagent's prompt to what it needs (the relevant phase `.md`, not the whole run history) — this
  is the token-economy policy from `CLAUDE.md`.
