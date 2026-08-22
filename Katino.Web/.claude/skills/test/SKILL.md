---
name: test
description: Runs only the tester stage against already-implemented code — writes/runs tests or does a manual E2E check via /run. Use for a standalone test pass without the full /implement pipeline.
argument-hint: <what to test / area of the recent change>
allowed-tools: Agent
model: inherit
---

Spawn the `tester` subagent (Agent tool, `subagent_type: tester`) with:

$ARGUMENTS

(If empty, tell it to test the current uncommitted diff, `git diff`.)

Print its result (what was tested, how, pass/fail, and the test-project-gap note if it applies) back to
the user.
