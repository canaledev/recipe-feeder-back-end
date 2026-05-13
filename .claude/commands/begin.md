# Begin Work on a Ticket

Run this before writing any code for an existing issue.
Load `LAB_NOTES_GIT.md` and `LAB_NOTES_POWERSHELL.md` before any command.

$ARGUMENTS: issue number or title (e.g. `/begin 12` or `/begin "save recipe feature"`)

---

## Phase 0: Read the issue

Fetch the full issue content:
```powershell
$env:PATH = "C:\Program Files\nodejs;" + $env:PATH
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
& "C:\Program Files\GitHub CLI\gh.exe" issue view <number> --repo canaledev/recipe-feeder-front-end
```

Read every field: title, body, acceptance criteria, technical notes, dependencies.
Note any terms or requirements that are ambiguous or underspecified.

---

## Phase 1: Dual analysis (run BEFORE asking or planning)

### Functional analysis
- Re-read `CHANGELOG.md` — what related features are already shipped? What is the current product state in this area?
- Trace the user journey: which screens does the user go through to reach this feature? What do they see before and after?
- Identify all product states the feature must handle: initial, loading, success, empty, error, edge cases mentioned in the issue.
- Check if the issue's acceptance criteria cover all these states — note any gaps.

### Technical analysis
- Identify the files that will likely need to change:
  - Feature folder: `src/app/features/<related>/`
  - Shared components: `src/app/components/`
  - Shared hooks: `src/app/hooks/`
  - Types: `src/app/types/`
  - API client: `src/app/lib/api.ts`
  - Tests: `src/tests/`
- Read the relevant existing files to understand current patterns and naming conventions.
- Identify what can be reused vs what must be created from scratch.
- Check what TanStack Query hooks already exist for related data.
- Check the existing test structure for adjacent features to understand the test approach.
- Identify any constraints: TypeScript strict mode, ESLint rules, PWA requirements, mobile-first layout.

**Document your findings** in both dimensions before proceeding.

---

## Phase 2: Pre-implementation clarification

Send **one message** with all questions derived from your analysis.
Only ask what the issue does not answer and what the code raises.

**Structure:**

```
I've read the issue and analyzed the codebase. Before I start I want to clarify a few things:

**From the functional analysis:**
[Findings: e.g. "The issue describes the happy path but doesn't specify the empty state when
the user has no saved recipes. The loading state is also not described."]

**From the technical analysis:**
[Findings: e.g. "The Recipe type in types/recipe.ts doesn't have a 'saved' field yet.
Adding it means the mock data and tests need to be updated too.
I also found that there's no existing hook for favorites — I'd need to create one."]

**Questions:**
1. [Product Q — e.g. "What should the empty state for saved recipes show? Just a message,
   or a CTA pointing to the feed?"]
2. [Product Q — e.g. "Should the save action give haptic feedback on mobile?"]
3. [Technical Q — e.g. "Should I add 'saved: boolean' to the Recipe type, or use a separate
   local collection so the backend contract isn't changed in this ticket?"]
4. [Any other ambiguity from the issue text]
```

Do NOT write any code until the user answers.

---

## Phase 3: Implementation plan

After receiving answers, present the full plan:

```
Implementation plan for: [issue title]

**Files to create:**
- src/app/features/<x>/<Component>.tsx
- src/tests/app/features/<x>/<Component>.test.tsx

**Files to modify:**
- src/app/types/recipe.ts — add [field]
- src/app/lib/api.ts — add [endpoint call]
- [other files]

**Order of work (TDD):**
1. Write failing test for [first acceptance criterion]
2. Implement [component/hook]
3. Write failing test for [second criterion]
4. Implement [next piece]
5. Run `npm run test:run` and `npm run build` to confirm all green

**Assumptions I'm making:**
- [List any assumptions not confirmed by the user]
```

Ask: "Does this plan look right? Any changes before I start?"
Do NOT write code until the user confirms the plan.

---

## Phase 4: Create branch (if not already on one)

```powershell
git checkout -b feature/<name>
```
Verify with `git branch` that you are on the feature branch before touching any file.
