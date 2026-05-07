# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Behavioral Guidelines

These rules govern how Claude approaches every task in this codebase. They override Claude's default behaviors and must be applied before writing any code.

### Think Before Coding

- Before implementing, state your assumptions explicitly. If a request is ambiguous, present the two most likely interpretations and ask which one to proceed with — never silently choose.
- If a simpler approach exists, say so. **Push back when warranted.**
- If something is unclear, stop. Name what's confusing. Ask.
- Surface tradeoffs upfront. If there are two valid solutions, briefly name both and state the recommended one with the reason before proceeding.
- **Starting work on a ticket:** Always do a dual analysis (functional: what the endpoint does, consumer contract; technical: affected files, EF models, migrations needed) before writing any code, ask outstanding clarifying questions, and present an implementation plan for approval.

### Simplicity First

- Write the minimum code that solves the stated problem. Nothing speculative.
- Do not add unrequested features, optional parameters "for future use", premature abstractions, or error handling for cases that cannot happen.
- Three similar lines are better than a premature helper. A helper is only warranted when there are three or more actual call sites.
- If you write 200 lines and it could be 50, rewrite it before submitting.

> **Self-check heuristic:** "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### Surgical Changes

- Touch only what the task requires. Do not refactor unrelated code, rename unrelated symbols, or fix pre-existing issues that were not part of the request.
- Match the existing style of the surrounding code exactly (indentation, naming conventions, import order).
- Only remove imports or usings that *your specific changes* made unused.
- If you notice unrelated dead code, mention it — don't delete it.

> **Litmus test:** Every changed line should trace directly to the user's request.

### Goal-Driven Execution (TDD)

- Transform every task into a verifiable goal before writing implementation code: write a failing test first, then implement until it passes.
- For multi-step tasks, lay out the full plan using the `[Step] → verify: [check]` format and get agreement before executing step one.
- A PR is only ready when all tests pass (`dotnet test`) and the build succeeds (`dotnet build`).

### Project Workflow

- **Git:** One branch per feature or fix. Open a PR before merging. Never push directly to `main`.
- **Issues + Project Board:** Every significant change must be linked to a GitHub Issue before work begins. All issues must also be added to the [GitHub Project board](https://github.com/users/canaledev/projects/1) using `gh project item-add 1 --owner canaledev --url <issue-url>`. When starting work on an issue, move it to "In Progress" on the board; it moves to "Done" automatically when its PR is merged.
- **CHANGELOG:** Every PR that ships a feature, fix, or notable change **must** include an update to the `[Unreleased]` section of `CHANGELOG.md` in the same branch and commit. This is not optional and must be done before opening the PR.
- **Git permissions:** Execute all non-destructive git/gh operations (commit, push, PR create, issue create, checkout, `gh project item-add`) without asking for confirmation. Only confirm: merge to `main`, branch deletion, and repo deletion.
- **CHANGELOG first:** Before asking the user what features exist in the app, read `CHANGELOG.md` — it is the source of truth for implemented features.
- **Task completion on a feature branch:** When reporting a task as done while on a feature branch (not `main`), always run `/ship` before closing — never mark done without creating the PR.

### Code Quality

- **Comments:** Always write comments explaining *what* and *why* for non-trivial logic. This project requires comments and overrides Claude's default no-comment behavior.
- **Language:** All code, variable names, commits, PR titles, issue titles, and inline comments must be in English. `CHANGELOG.md` and `README.md` must follow standard best practices.
- **Naming:** Prefer precise, descriptive names over short ones. Avoid abbreviations unless they are industry-standard (e.g., `id`, `url`, `ctx`).

---

## Project Overview

**Feedy Backend** is a .NET 8 Web API that powers the Feedy mobile-first PWA — a personalized recipe discovery app. Its core responsibility is the **Smart Feed engine**: ranking recipes against each user's psychographic and nutritional profile to produce a `matchPercentage` score.

**Frontend:** React 18 + Vite PWA (separate repository — `recipe-feeder-front-end`).

## Tech Stack

- **.NET 8 Web API** — Minimal APIs preferred over MVC controllers
- **Dapper** — micro-ORM for all database access; raw SQL mapped to domain models
- **SQL Server / PostgreSQL** (TBD) — relational store for recipes, users, and profiles
- **JWT Bearer** — authentication tokens consumed by the frontend
- **xUnit + FluentAssertions** — unit and integration tests

## Commands

```powershell
# Always prepend PATH in PowerShell if dotnet is not found:
# $env:PATH = "C:\Program Files\dotnet;" + $env:PATH

dotnet build                          # Build the solution
dotnet run --project src/Feedy.Api    # Run the API (default: http://localhost:5000)
dotnet watch --project src/Feedy.Api  # Run with hot reload
dotnet test                           # Run all tests
dotnet test --filter "FullyQualifiedName~RecipeService"  # Run a single test class
dotnet ef migrations add <Name>       # Add a new EF migration
dotnet ef database update             # Apply pending migrations
```

## Architecture

The solution follows **Clean Architecture** with four layers. Dependencies point inward only — outer layers depend on inner layers, never the reverse.

### Layer responsibilities

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Feedy.Domain` | Entities, value objects, domain interfaces (no framework deps) |
| Application | `Feedy.Application` | Use cases, DTOs, service interfaces, business rules |
| Infrastructure | `Feedy.Infrastructure` | Dapper repositories, JWT, external services |
| Presentation | `Feedy.Api` | Minimal API endpoints, DI wiring, `Program.cs` |

### Planned folder structure

```
src/
├── Feedy.Domain/
│   ├── Entities/           ← Recipe, User, UserProfile, Playlist
│   └── Interfaces/         ← IRecipeRepository, IUserRepository, etc.
├── Feedy.Application/
│   ├── UseCases/           ← one folder per use case (GetFeed, RegisterUser, …)
│   └── DTOs/               ← request/response contracts
├── Feedy.Infrastructure/
│   ├── Persistence/        ← Dapper repository implementations + raw SQL
│   └── Auth/               ← JWT issuance and validation
└── Feedy.Api/
    ├── Endpoints/          ← MapXxxEndpoints() extension methods, one file per feature
    └── Program.cs
tests/
├── Feedy.Domain.Tests/
├── Feedy.Application.Tests/
└── Feedy.Infrastructure.Tests/
```

### Key architectural decisions

- **Clean Architecture + SOLID.** Dependencies always point inward. The Domain layer has zero framework references. Use cases depend only on interfaces, never on concrete infrastructure.
- **Repository pattern via interfaces.** `IRecipeRepository`, `IUserRepository`, etc. are defined in `Feedy.Domain`; their Dapper implementations live in `Feedy.Infrastructure`. This keeps SQL out of the application layer and makes use cases unit-testable with mocks.
- **Dapper for all data access.** SQL queries are written explicitly in repository implementations. No query builders — keep SQL readable and auditable.
- **Minimal APIs, not Controllers.** Route handlers live in `MapXxxEndpoints()` extension methods, one file per feature area.
- **Smart Feed engine** lives in `Feedy.Application/UseCases/Feed/FeedRankingService.cs`. It reads the user's profile (flavor tags, dietary regime, rejected ingredients) and scores each recipe; the score is exposed as `matchPercentage` (0–100).
- **All responses are camelCase JSON** to match the TypeScript frontend contract exactly.
- **KISS / DRY / no code smells.** Prefer simple, readable code. Extract only when there are three or more actual call sites. Avoid primitive obsession, long parameter lists, and feature envy.

### Core data contract

The `Recipe` response shape must match the frontend's TypeScript interface:

```csharp
// RecipeResponse.cs
record RecipeResponse(
    Guid Id,
    string Title,
    string Description,
    int MatchPercentage,      // 0–100, computed by FeedRankingService
    string[] Tags,
    string Difficulty,        // "Easy" | "Medium" | "Hard"
    int PrepTimeMinutes,
    bool IsSubscribedSequence,
    string MainImageUrl
);
```

### Core features (MVP)

1. **Auth** — `POST /auth/register`, `POST /auth/login` → returns JWT
2. **Recipes** — `GET /recipes` (paginated), `GET /recipes/{id}`
3. **Smart Feed** — `GET /feed` → ranked recipe list with `matchPercentage`
4. **User Profile** — `GET/PUT /profile` — flavor tags, dietary regime, rejected ingredients

---

> **Lab notes by domain** — load on demand with `/lab`:
> - `LAB_NOTES_POWERSHELL.md` — PATH setup, Unix equivalents, multiline strings (load when: running dotnet/gh)
> - `LAB_NOTES_GIT.md` — git status, gh project scope, branch-first rule (load when: git/GitHub workflow)
