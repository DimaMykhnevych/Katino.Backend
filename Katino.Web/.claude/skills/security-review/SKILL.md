---
name: security-review
description: Runs a full security review of the current diff — the built-in security-review skill for generic OWASP findings, then the repo-specific security-reviewer subagent for JWT audience/rate-limiting/injection findings specific to this repo. Use for a standalone security pass without the full /implement pipeline.
argument-hint: [plan or context for the diff being reviewed]
allowed-tools: Skill, Agent
model: inherit
---

Two-pass security review of the current diff (`git diff`, or whatever scope the user specifies):

1. Run the built-in `security-review` skill (Skill tool, `skill: security-review`) for generic
   OWASP-pattern coverage.
2. Spawn the `security-reviewer` subagent (Agent tool, `subagent_type: security-reviewer`) with the diff
   context and, if given, `$ARGUMENTS` — this covers repo-specific concerns (JWT audience boundaries
   between CRM/storefront, rate limiting on new public endpoints, this repo's client-input trust
   boundary) that the generic pass doesn't know about. Don't ask it to repeat the generic scan.

Merge both sets of findings into one list for the user, deduplicating if both passes flagged the same
thing. Don't auto-fix anything — this skill only reports. Treat any finding touching auth/audience
boundaries as high-priority to surface clearly, given this is a production system.
