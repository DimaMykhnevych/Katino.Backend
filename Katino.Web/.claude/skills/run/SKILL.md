---
name: run
description: Boots the CRM or storefront app locally (dotnet run) in the background, waits until it's actually listening, and hands back the base HTTPS URL for curl/Swagger use. Use when a task needs to hit a real running endpoint instead of just reading code.
argument-hint: <crm|site>
allowed-tools: Bash, PowerShell, Monitor
model: sonnet
---

Target from `$ARGUMENTS` — must be `crm` or `site`:

| Target | Project | HTTPS URL | Port |
|---|---|---|---|
| `crm` | `Katino.Web/Katino.Web.csproj` | `https://localhost:7107` | `7107` |
| `site` | `site/Katino.Store.Web/Katino.Store.Web.csproj` | `https://localhost:7291` | `7291` |

(Values come from each project's `Properties/launchSettings.json` — re-check that file instead of this
table if a call ever looks wrong, in case ports were changed.)

If `$ARGUMENTS` is neither, ask which one before doing anything.

## Steps

1. **Claim the port.** Find and kill whatever already holds the target port before starting — always do
   this, even if you don't know whether something's there, so a stale process from a previous session
   never causes a silent conflict:
   ```powershell
   $conn = Get-NetTCPConnection -LocalPort <port> -State Listen -ErrorAction SilentlyContinue
   if ($conn) { Stop-Process -Id $conn.OwningProcess -Force -ErrorAction SilentlyContinue }
   ```
2. **Start it.** Run `dotnet run --project <path> --launch-profile https` in the background (Bash
   `run_in_background: true`), from the repo config root
   (`C:\Dima\Projects\Katino.Backend\Katino.Web`).
3. **Wait for readiness.** Don't return the URL until you've actually seen `Now listening on` in the
   process output — use the Monitor tool to watch the background shell's output rather than guessing with
   a fixed sleep. If it hasn't come up within a reasonable window (~60s), treat that as failure and go to
   the failure step below instead of returning a URL that isn't live yet.
4. **Hand back the base URL** (`https://localhost:<port>`) so the caller can `curl -sk` against it or hit
   `/swagger`. Note that `curl` needs `-sk` (self-signed dev cert) against these.
5. **On failure**: surface the captured stdout/stderr from the background process (via Monitor / the
   background task's output) verbatim so the caller can see the actual startup error — don't just say "it
   failed."
6. **On cleanup** (when the caller's task is done, or at the start of the next `/run` call for the same
   target): kill whatever process now holds the port, the same way as step 1. Don't rely on tracking the
   background-shell handle to know what to stop — killing by port is what makes this idempotent across
   separate `/run` invocations, including from a different session that doesn't have the original
   background-task handle.

## Known limitation

Killing "whatever holds the port" is blunt — it doesn't check that the process is actually a `dotnet run`
for this project before killing it. Acceptable for local dev ports 7107/7291, but mention this if asked to
extend `/run` to a port that might legitimately be used by something else.
