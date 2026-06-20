# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

A B2B General Ledger ERP web app with an automated risk-approval workflow. It is a **CV/portfolio
project** (target roles: ERP/.NET, e.g. Acumatica). Built from a static design (`design/*.html`)
into a full-stack app.

## Live / links
- App: https://autoledger.fly.dev (Fly app `autoledger` + Postgres `autoledger-db`, region `fra`)
- Repo: https://github.com/vukasin-djuricic/autoledger (public, branch `main`)
- Fly account used for deploy: **peraprdasmrda123@gmail.com**, org `personal` (NOT estrobann).
- Demo logins: `controller@autoledger.local` / `clerk@autoledger.local`, password `Passw0rd!`
  (or the one-click buttons on the sign-in page).

## Conventions (important)
- **Do all tooling in Docker — nothing installed locally.** Only Docker is available; local .NET
  SDKs are 6 & 7 (EOL). Target **.NET 9** via the SDK image regardless.
- **No AI attribution in commits.** Never add a `Co-Authored-By: Claude ...` trailer; keep commit
  messages clean and professional.
- All user-facing text and repo content is in **English**; chat with the user in **Serbian**.

## Architecture (3 layers, maps to the job spec)
- `src/AutoLedger.Domain` — entities, enums, **State pattern** (`States/`), **Strategy pattern**
  risk (`Risk/`), services (`PostingService`, `RiskAssessmentService`, `WorkflowEngine`,
  `YearEndCloseService`), interfaces (`Abstractions/`). No infrastructure dependencies.
- `src/AutoLedger.Infrastructure` — EF Core 9 + Npgsql, repositories + Unit of Work, raw-SQL
  analytics (`Queries/`), audit `SaveChanges` interceptor, `xmin` optimistic concurrency, Identity,
  `DbSeeder`.
- `src/AutoLedger.Web` — ASP.NET Core MVC, Razor views/partials ported from `design/`, view
  components, Identity (roles `Clerk`/`Controller`), compiled Tailwind.
- `tests/AutoLedger.Tests` — xUnit (double-entry, state machine, risk strategies, posting/rollback).

**Where the patterns converge:** `WorkflowEngine.SubmitAsync` is the orchestration entry point — it
scores risk (Strategy), drives `JournalEntry` through its lifecycle (State), and posts atomically
(Unit of Work). Low-risk entries auto-approve & post; risky ones stop at `PendingReview` for a
controller. Start here to understand how a transaction flows end to end.

**Accounting-cycle features:** posted entries are immutable — corrections go through
`WorkflowEngine.ReverseAsync` (storno; self-FK `ReversalOf`/`ReversedBy`). Rejected entries can be
reopened to Draft and resubmitted. `PostingService` blocks posting into a **closed `FiscalPeriod`**
(so reversals and year-end close respect the lock too). `YearEndCloseService` zeroes P&L accounts
into Retained Earnings (`3200`). Reports (Income Statement, Balance Sheet, account-ledger drill-down)
are raw SQL in `LedgerQueries`. `DbSeeder.ResetAsync` wipes + reseeds for the Settings reset button.

**Non-goals (deliberate scope):** no AP/AR sub-ledgers (open-item/aging), no multi-currency/FX, no
multi-tenancy. Keep these out unless explicitly asked.

## Common commands (run via Docker)
```bash
# run the full stack locally
docker compose up --build            # → http://localhost:8080

# build / test
docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 dotnet build AutoLedger.sln
docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 dotnet test

# run a single test / class (filter by fully-qualified name or method)
docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 \
  dotnet test --filter "FullyQualifiedName~JournalEntryTests"

# EF migration
docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 \
  bash -c "dotnet tool restore && dotnet ef migrations add <Name> \
    --project src/AutoLedger.Infrastructure --startup-project src/AutoLedger.Infrastructure"

# rebuild Tailwind stylesheet (output: src/AutoLedger.Web/wwwroot/css/site.css)
docker run --rm -v "$PWD":/src -w /src/src/AutoLedger.Web node:20-alpine \
  npx -y tailwindcss@3.4.17 -i ./Styles/app.css -o ./wwwroot/css/site.css --minify

# deploy (CI does this automatically on push to main; FLY_API_TOKEN secret is set)
flyctl deploy -a autoledger --remote-only
```

## Gotchas already handled
- Fly's flycast Postgres `DATABASE_URL` uses `?sslmode=disable`; `DatabaseConfiguration.FromUri`
  honours the sslmode (forcing SSL crashes the app). Don't revert that.
- `xmin` is mapped as the concurrency token but must NOT be created as a column in migrations
  (it's a Postgres system column) — see the note in the InitialCreate migration.
- Tailwind classes built dynamically in C# (`StatusBadgeViewComponent`) and in Razor must be full
  literal strings so the Tailwind content scanner (cshtml + Components/*.cs) picks them up.

## Status: complete and deployed. CI/CD green (build → test → deploy to Fly).
