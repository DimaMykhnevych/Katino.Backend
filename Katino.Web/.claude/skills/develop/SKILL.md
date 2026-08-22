---
name: develop
description: Runs only the developer stage — implements a plan that's already been decided/confirmed. Use for standalone implementation when you already have a plan in hand (e.g. from /architect), without the full /implement pipeline's other stages.
argument-hint: <plan or task description>
allowed-tools: Agent
model: inherit
---

Spawn the `developer` subagent (Agent tool, `subagent_type: developer`) with:

$ARGUMENTS

If this doesn't look like an already-confirmed plan (just a bare task description instead), ask the user
to confirm before proceeding — the `developer` agent will implement whatever it's handed without a design
review, so a plan should already have been reasoned about (via `/architect` or equivalent) before this
runs. This is a lighter-weight entry point than `/implement`; it does not enforce the plan-confirmation or
migration-confirmation gates that `/implement` does, so use it only when you've already made those calls
yourself.

Print the developer's summary (files changed, build result, migration-pending flag if any) back to the
user.
