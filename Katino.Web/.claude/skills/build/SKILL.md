---
name: build
description: Builds a Katino.Backend project (CRM, storefront, or the whole solution) via dotnet build and reports a short, noise-free summary. Use when the user asks to build, compile, or check that the code compiles.
argument-hint: [crm|site|all]
allowed-tools: Bash
model: sonnet
---

Build target from `$ARGUMENTS` (default `all` if empty):
- `crm` → `Katino.Web/Katino.Web.csproj`
- `site` → `site/Katino.Store.Web/Katino.Store.Web.csproj`
- `all` (or anything else/empty) → `Katino.Web.sln`

Run `dotnet build <target> --nologo` from the repo config root
(`C:\Dima\Projects\Katino.Backend\Katino.Web`).

## Summarizing the output

This repo's build output is full of NuGet vulnerability-advisory warnings that are noise for this
purpose — **do not show these to the user, and do not count them as reasons to call the build
unhealthy**:

- `NU1903`, `NU1902` — known package vulnerability warnings
- `NU1510` — unnecessary package reference warnings

Filter lines containing those codes out of what you report. Then report, concisely:
- Pass/fail (build succeeded / failed)
- Any real compiler errors (`CS####`) or warnings that aren't in the filtered set above — these are the
  ones that actually matter, show them in full with file:line
- One-line count summary (e.g. "0 errors, 2 warnings (NuGet advisories hidden)")

If the build fails, do not attempt to fix it yourself unless the user explicitly asks — this skill's job
is to build and report, not to debug. A calling agent/pipeline (e.g. `/implement`'s developer step) is
responsible for fixing its own build breaks.
