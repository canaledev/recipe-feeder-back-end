# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Behavioral Guidelines

These rules govern how Claude approaches every task in this codebase. They override Claude's default behaviors and must be applied before writing any code.

### Think Before Coding

- Before implementing, state your assumptions explicitly. If a request is ambiguous, present the two most likely interpretations and ask which one to proceed with — never silently choose.
- If a simpler approach exists, say so. **Push back when warranted.**
- If something is unclear, stop. Name what's confusing. Ask.
- If something in the codebase is confusing, say so. Do not work around confusion; surface it.
- Surface tradeoffs upfront. If there are two valid solutions, briefly name both and state the recommended one with the reason before proceeding.
- **Creating a user story:** Always do a dual analysis first (functional: product state, user journey; technical: affected files, reusable components, API/type impact), then conduct a requirements interview with targeted questions derived from that analysis. Use `/start-feature`.
- **Starting work on a ticket:** Always do the same dual analysis on the issue before writing any code, ask any outstanding clarifying questions, and present an implementation plan for approval. Use `/begin`.

### Simplicity First

- Write the minimum code that solves the stated problem. Nothing speculative.
- Do not add unrequested features, optional parameters "for future use", premature abstractions, or error handling for cases that cannot happen.
- Three similar lines are better than a premature helper. A helper is only warranted when there are three or more actual call sites.
- If you write 200 lines and it could be 50, rewrite it before submitting.

> **Self-check heuristic:** "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### Surgical Changes

- Touch only what the task requires. Do not refactor unrelated code, rename unrelated symbols, or fix pre-existing issues that were not part of the request.
- Match the existing style of the surrounding code exactly (indentation, naming conventions, import order).
- Only remove imports or functions that *your specific changes* made unused — not pre-existing dead code.
- If you notice unrelated dead code, mention it - don't delete it.

> **Litmus test:** Every changed line should trace directly to the user's request.

### Goal-Driven Execution (TDD)

- Transform every task into a verifiable goal before writing implementation code: write a failing test first, then implement until it passes.
  - *Example 1: "Add validation" → "Write tests for invalid inputs, then make them pass"*
  - *Example 2: "Fix the bug" → "Write a test that reproduces it, then make it pass"*
- For multi-step tasks, lay out the full plan using the `[Step] → verify: [check]` format and get agreement before executing step one.
- Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.
- A PR is only ready when all tests pass (`npm run test:run`) and the production build succeeds (`npm run build`).

### Project Workflow

- **Git:** One branch per feature or fix. Open a PR before merging. Never push directly to `main`.
- **Issues + Project Board:** Every significant change must be linked to a GitHub Issue before work begins. All issues must also be added to the [GitHub Project board](https://github.com/users/canaledev/projects/1) using `gh project item-add 1 --owner canaledev --url <issue-url>`. When starting work on an issue, move it to "In Progress" on the board; it moves to "Done" automatically when its PR is merged.
- **CHANGELOG:** Every PR that ships a feature, fix, or notable change **must** include an update to the `[Unreleased]` section of `CHANGELOG.md` in the same branch and commit. This is not optional and must be done before opening the PR — never after. The CHANGELOG is the source of truth for what has been shipped.
- **Dev server:** Start the Vite dev server without asking for confirmation whenever the user requests it.
- **Git permissions:** Execute all non-destructive git/gh operations (commit, push, PR create, issue create, checkout, `gh project item-add`) without asking for confirmation. Only confirm: merge to `main`, branch deletion, and repo deletion.
- **CHANGELOG first:** Before asking the user what features exist in the app, read `CHANGELOG.md` — it is the source of truth for implemented features.
- **Task completion on a feature branch:** When reporting a task as done while on a feature branch (not `main`), always run `/ship` before closing — never mark done without creating the PR.

### Code Quality

- **Comments:** Always write comments explaining *what* and *why* for non-trivial logic. This project requires comments and overrides Claude's default no-comment behavior.
- **Language:** All code, variable names, commits, PR titles, issue titles, and inline comments must be in English. `CHANGELOG.md` and `README.md` must follow standard best practices.
- **Naming:** Prefer precise, descriptive names over short ones. Avoid abbreviations unless they are industry-standard (e.g., `id`, `url`, `ctx`).

---

## Project Overview

**Feedy** is a mobile-first PWA (Progressive Web App) for personalized recipe discovery. The core feature is a "Smart Feed" that surfaces recipes algorithmically based on the user's psychographic and nutritional profile. The backend is a separate .NET 8 service.

## Tech Stack

- **React 18 + Vite + TypeScript**
- **Tailwind CSS** — configured via `tailwind.config.js` with design tokens (organic palette: emerald greens, terracottas, ochres)
- **TanStack Query** — all server state and API synchronization
- **React Context API** — global client-side state only
- **Headless UI or Radix UI** — unstyled base components
- **Lucide-React** (functional icons) + **Phosphor Icons** (decorative icons)
- **Bitter** (Google Fonts, serif) for all headings; **Inter or Montserrat** for body text

## Commands

```bash
npm run dev          # Start development server (Vite root: src/app)
npm run build        # TypeScript check + production build → dist/
npm run preview      # Preview production build locally
npm run lint         # ESLint
npm run test         # Run tests in watch mode (Vitest)
npm run test:run     # Run tests once and exit
npm run test:run src/tests/lib/api.test.ts   # Run a single test file
```

> **Environment:** On this machine, `npm` and `node` are at `C:\Program Files\nodejs\` and `gh` at `C:\Program Files\GitHub CLI\gh.exe`. Neither is in the bash PATH — always use PowerShell and prepend `$env:PATH = "C:\Program Files\nodejs;" + $env:PATH` before running npm commands.

## Architecture

The codebase uses a **feature-based folder structure** inside `src/app/features/<feature>/`. Each feature owns its components, hooks, and types. Shared primitives live in `src/app/components/` and `src/app/hooks/`.

### Folder structure

```
src/
├── app/        ← deployable application (Vite root points here)
│   ├── contexts/       AppContext.tsx (Provider only) + useAppContext.ts (hook only)
│   ├── features/       feed/ · onboarding/ · recipe/
│   ├── components/     shared UI primitives
│   ├── hooks/          shared custom hooks
│   ├── lib/            api.ts — centralized API client
│   ├── types/          recipe.ts — core data contracts
│   ├── App.tsx · main.tsx · index.css · index.html · vite-env.d.ts
└── tests/      ← all test files (mirroring app/ structure)
    ├── app/    · lib/  · contexts/
    └── setup.ts
```

### Key architectural decisions

- **All API calls go through a centralized API client** (`src/lib/api.ts` or similar) that talks exclusively to the .NET 8 backend. No feature should call `fetch` directly.
- **TanStack Query owns all server state.** React Context is for client-only state (e.g., onboarding step, UI preferences).
- **Mobile-first, max-w-md on desktop.** The root layout constrains the main container to `max-w-md` — this is intentional to preserve mobile ergonomics on desktop.
- **PWA requirements:** `public/manifest.json` and Service Workers must be maintained to support offline data persistence and home-screen installation.
- **Lazy loading** for all media assets; custom skeleton components to prevent layout shift.

### Core data contract

The central `Recipe` type (matches backend API response):

```ts
interface Recipe {
  id: string;               // UUID
  title: string;            // Rendered in Bitter font
  description: string;
  matchPercentage: number;  // 0–100 affinity score from Smart Feed engine
  tags: string[];
  difficulty: 'Easy' | 'Medium' | 'Hard';
  prepTimeMinutes: number;
  isSubscribedSequence: boolean;
  mainImageUrl: string;
}
```

### Core features (MVP)

1. **Onboarding** — profile setup: flavor preference, rejected ingredients (predictive search + tag chips), dietary regime (Vegan, Keto, Celiac, Paleo, etc.)
2. **Smart Feed** — swipeable recipe cards with a `matchPercentage` badge; subscription to curated meal sequences
3. **Recipe Detail** — hands-free reading mode (screen-wake lock), ingredient checklist, nutritional/complexity summary

## Design System

- Titles must use **Bitter** (serif) at multiple weights to establish editorial hierarchy.
- Color palette is organic: emerald greens, terracottas, ochres — defined as Tailwind design tokens in `tailwind.config.js`.
- Micro-interactions and haptic feedback are required on critical actions (swipe gestures, ingredient check-off).

---

> **Lab notes by domain** — load on demand with `/lab`:
> - `LAB_NOTES_BUILD.md` — tsconfig, ESLint, @types/node, Vitest root (load when: build/lint/test errors)
> - `LAB_NOTES_POWERSHELL.md` — PATH setup, Unix equivalents, multiline strings (load when: running npm/node/gh)
> - `LAB_NOTES_GIT.md` — git status, gh project scope, branch-first rule (load when: git/GitHub workflow)
