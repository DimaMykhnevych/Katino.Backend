---
name: architect
description: Runs only the architect stage — produces an implementation plan without building anything. Use for a standalone design/planning pass, without the full /implement pipeline.
argument-hint: <task description>
allowed-tools: Agent
model: inherit
---

Spawn the `architect` subagent (Agent tool, `subagent_type: architect`) with the task description:

$ARGUMENTS

Print its plan to the user verbatim. Do not proceed to implementation — this skill only produces a plan.
If the user wants to act on it, they can run `/implement` (which re-runs the architect stage itself as
part of its confirmation-gated pipeline) or `/develop` with the plan pasted in.
