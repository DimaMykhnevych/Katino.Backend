---
name: review
description: Runs a full review of the current diff — the built-in code-review skill for generic correctness/simplification findings, then the repo-specific reviewer subagent for CQRS/Installer/plan-conformance findings. Use for a standalone review pass without the full /implement pipeline.
argument-hint: [plan or context for the diff being reviewed]
allowed-tools: Skill, Agent
model: inherit
---

Two-pass review of the current diff (`git diff`, or whatever scope the user specifies):

1. Run the built-in `code-review` skill (Skill tool, `skill: code-review`) at its default effort level for
   generic correctness/simplification/efficiency findings.
2. Spawn the `reviewer` subagent (Agent tool, `subagent_type: reviewer`) with the diff context and, if
   given, the plan/context from `$ARGUMENTS` — this covers repo-specific conventions (CQRS structure,
   Installer DI, migration discipline, plan conformance) that the generic pass doesn't know about. Don't
   ask it to repeat the generic scan.

Merge both sets of findings into one list for the user, deduplicating if both passes flagged the same
thing. Don't auto-fix anything — this skill only reports.
