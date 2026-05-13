# QA Expert Review

Perform a structured QA review of the current branch changes.
Inspired by:
- Kent C. Dodds – The Testing Trophy (kentcdodds.com/blog/the-testing-trophy-and-testing-classifications)
- Testing Library – Guiding Principles (testing-library.com/docs/guiding-principles)
- web.dev – Web Vitals & Accessibility (web.dev/articles/vitals · web.dev/learn/accessibility)
- Martin Fowler – The Practical Test Pyramid (martinfowler.com/articles/practical-test-pyramid)
- Playwright – Best Practices (playwright.dev/docs/best-practices)

Stack: React 18 · Vite · TypeScript · Vitest · React Testing Library · TanStack Query · Tailwind CSS

$ARGUMENTS: optional scope filter (e.g. `/qa feed` to focus on the feed feature)

---

## Phase 0: Collect evidence

Run these commands silently to gather data before forming any opinion.

```bash
# Diff of the branch vs main
git diff main...HEAD --name-only
git diff main...HEAD
```

Read every changed file fully. Also read the corresponding test file for each changed
source file, even if the test file itself was not modified.

If $ARGUMENTS is set, filter the review to files matching that scope. Otherwise review all changed files.

---

## Phase 1: Test quality analysis

**Philosophy (Kent C. Dodds / Testing Library):**
> "The more your tests resemble the way your software is used, the more confidence they can give you."

For each test file in the diff, evaluate:

### 1.1 Test trophy balance
- Are there integration tests (component + real hooks/context interaction) for user-facing behavior?
- Are unit tests limited to pure logic (utils, helpers) where isolation makes sense?
- Are there E2E-level concerns that should be flagged as out-of-scope for this PR?

### 1.2 React Testing Library query priority
Check queries used. Violations of this priority order are findings:
```
✅ getByRole()          — tests semantic HTML; inherently tests accessibility
✅ getByLabelText()     — tests form labels; what the user sees
✅ getByPlaceholderText()
✅ getByText()
✅ getByDisplayValue()
⚠️  getByAltText()      — only for images
⚠️  getByTitle()        — inconsistently read by screen readers
❌ getByTestId()        — last resort; signals missing semantic structure
```

### 1.3 Implementation detail testing (anti-patterns)
Flag any of the following as **blocking**:
- Asserting on component internal state or refs
- Importing and calling component methods directly
- Testing CSS class names instead of visible behavior
- Using `wrapper.instance()` or `wrapper.state()` (Enzyme patterns)
- Mocking React internal lifecycle (`useState`, `useEffect` directly)

### 1.4 Coverage of states
For each component or feature changed, verify tests exist for:
- [ ] Happy path (normal usage)
- [ ] Loading state (skeleton, spinner)
- [ ] Empty state (no data returned)
- [ ] Error state (API failure, validation error)
- [ ] Edge cases specific to this feature (from the issue's acceptance criteria)

### 1.5 Async patterns
- `findBy*` or `waitFor()` used for async assertions — not arbitrary `setTimeout` or `sleep`
- No `act()` warnings in tests (signals missing async handling)

### 1.6 Mocking discipline
- External API calls mocked via `vi.stubGlobal` or `vi.mock` — not real network calls
- TanStack Query mocked at the hook level, not at the component level
- No over-mocking: components should integrate with real hooks where possible
- `vi.stubGlobal('fetch', vi.fn())` — never `global.fetch = vi.fn()` (lab note #1)

---

## Phase 2: Accessibility (A11Y)

**Reference: web.dev/learn/accessibility — automated tools catch ~50% of issues**

### 2.1 Semantic HTML
- Are native HTML elements used instead of generic divs with ARIA?
  - Buttons: `<button>` not `<div onClick>`
  - Navigation: `<nav>`, `<ul>/<li>` not nested divs
  - Forms: `<form>`, `<label>`, `<input>` — not custom widgets
  - Headings: proper hierarchy (`h1` → `h2` → `h3`), not skipped levels
  - Images: `<img alt="...">` with meaningful alt text (empty `alt=""` for decorative images)

### 2.2 Keyboard navigation
- All interactive elements reachable via Tab
- Focus order is logical (matches visual order)
- Custom interactive components (swipe cards, carousels) have keyboard alternatives
- No `tabIndex > 0` (breaks natural tab order)
- Focus visible on all focusable elements — not hidden with `outline: none` without replacement

### 2.3 ARIA
- ARIA used only when semantic HTML is insufficient
- `aria-label` or `aria-labelledby` on icon-only buttons
- Dynamic regions that update (toasts, alerts) use `aria-live`
- No redundant ARIA (e.g., `role="button"` on a `<button>`)

### 2.4 Color and contrast
- No information conveyed by color alone (error states use icon + text, not just red)
- Tailwind color tokens used — check against design system (emerald, terracotta, ochre) for WCAG AA compliance:
  - Normal text: contrast ratio ≥ 4.5:1
  - Large text (≥18pt / ≥14pt bold): ≥ 3:1
  - UI components and focus indicators: ≥ 3:1

### 2.5 Forms (Onboarding screens)
- Every `<input>` has an associated `<label>` (via `htmlFor` / `id` or `aria-label`)
- Error messages linked to inputs via `aria-describedby`
- Required fields marked with `aria-required` or `required` attribute

---

## Phase 3: React and TypeScript patterns

### 3.1 TypeScript strictness
- No `any` types introduced
- No `// @ts-ignore` without a comment explaining why
- Props interfaces fully typed (no implicit `{}` or `object`)
- Return types explicit on hooks and utility functions

### 3.2 React patterns
- `useEffect` always has a dependency array (never omitted)
- `useCallback` / `useMemo` used only where there is a measurable performance reason, not pre-emptively
- Keys in lists are stable identifiers (never array index when list can reorder or filter)
- No prop mutation — all props treated as read-only

### 3.3 State management (architecture rules)
- Server state managed by TanStack Query — not duplicated into Context or useState
- React Context used only for client-only UI state (onboarding step, UI preferences)
- No direct `fetch()` calls in components — all API calls go through `src/app/lib/api.ts`

### 3.4 Component structure
- New components placed in the correct folder:
  - Feature-specific: `src/app/features/<feature>/`
  - Shared primitive: `src/app/components/`
  - Shared hook: `src/app/hooks/`
- No mixing of context objects and components in the same `.tsx` file (ESLint lab note #2)

---

## Phase 4: Performance

**Reference: web.dev/articles/vitals — Core Web Vitals targets**

### 4.1 Core Web Vitals risks
Flag any changes that could regress:
- **LCP ≤ 2.5s** — new above-the-fold images without lazy loading; large JS bundles added
- **INP ≤ 200ms** — synchronous heavy computation in event handlers; blocking renders
- **CLS ≤ 0.1** — images/media without explicit `width`/`height`; dynamic content injected above existing content

### 4.2 Images and media
- All `<img>` tags use `loading="lazy"` except above-the-fold hero images
- Explicit `width` and `height` attributes to prevent layout shift
- `mainImageUrl` from Recipe type rendered with correct aspect ratio container

### 4.3 Bundle impact
- No large new dependencies added without justification
- Imports are specific (`import { X } from 'lib'`) not wildcard (`import * from 'lib'`)
- New features behind lazy-loaded routes or components where appropriate

### 4.4 PWA considerations
- Service Worker not broken by new routes (check `public/manifest.json` if routes changed)
- Offline-relevant data appropriately cached
- No API calls in render without TanStack Query's caching layer

---

## Phase 5: Mobile-first and design system

### 5.1 Layout
- All new UI is mobile-first (styles written for small screen, `md:` / `lg:` variants for larger)
- Root layout constraint respected: `max-w-md` on the main container — no UI that breaks this
- No horizontal scroll on mobile introduced

### 5.2 Design system compliance
- Headings use **Bitter** serif — enforced via Tailwind `font-bitter` token
- Body text uses **Inter** or **Montserrat**
- Colors from the organic palette only: emerald greens, terracottas, ochres — defined in `tailwind.config.js`
- No hardcoded hex values or raw Tailwind colors outside the design tokens

### 5.3 Micro-interactions (where required)
- Swipe gestures on critical actions include haptic feedback (`navigator.vibrate`)
- Ingredient check-off animation present if that feature is in scope
- Loading skeletons used — no flash of blank content (CLS risk)

---

## Phase 6: Code hygiene ("clean kitchen" check)

- No `console.log`, `console.error`, or `debugger` statements in production code
- No commented-out code blocks left behind
- No TODO/FIXME added without a linked GitHub issue number
- No temporary files (`pr-body.md`, `*.tmp`) committed
- `package-lock.json` committed alongside any `package.json` changes
- No `.env` values hardcoded in source files

---

## Report format

Produce the review in this structure:

```
# QA Review — [branch name] — [date]

## Overall verdict
[ PASS | NEEDS WORK | BLOCKING ]

One sentence summary.

---

## Blocking issues (must fix before merge)
> Issues that break functionality, introduce regressions, or violate core architecture rules.

### [BLOCKING-1] [File:line] — [Short title]
**Finding:** [What is wrong]
**Why it matters:** [User impact or architecture violation]
**Fix:** [Specific, actionable instruction]

---

## Warnings (should fix, not blocking)
> Issues that degrade quality, maintainability, or user experience but don't break anything.

### [WARN-1] [File:line] — [Short title]
**Finding:** ...
**Fix:** ...

---

## Suggestions (optional improvements)
> Good-to-haves based on best practices.

### [SUG-1] [File:line] — [Short title]
**Finding:** ...
**Consider:** ...

---

## What's well done
- [Specific positive finding 1]
- [Specific positive finding 2]

---

## Checklist summary
| Area | Status | Notes |
|---|---|---|
| Test quality | ✅ / ⚠️ / ❌ | |
| RTL query priority | ✅ / ⚠️ / ❌ | |
| State coverage (loading/empty/error) | ✅ / ⚠️ / ❌ | |
| Accessibility (semantic HTML) | ✅ / ⚠️ / ❌ | |
| Accessibility (keyboard) | ✅ / ⚠️ / ❌ | |
| TypeScript strictness | ✅ / ⚠️ / ❌ | |
| React patterns | ✅ / ⚠️ / ❌ | |
| State management architecture | ✅ / ⚠️ / ❌ | |
| Performance / CWV risks | ✅ / ⚠️ / ❌ | |
| Mobile-first / design system | ✅ / ⚠️ / ❌ | |
| Code hygiene | ✅ / ⚠️ / ❌ | |
```

If verdict is PASS or NEEDS WORK: list what the developer must do before `/ship`.
If verdict is BLOCKING: list what must be fixed and offer to fix blocking issues directly.
