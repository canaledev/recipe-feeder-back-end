# Start Feature

Create a fully-specified GitHub Issue for a new feature or fix.
Load `LAB_NOTES_GIT.md` and `LAB_NOTES_POWERSHELL.md` before any command.

---

## Phase 0: Dual analysis (run BEFORE asking any question)

Given the feature name/description in $ARGUMENTS, silently explore both dimensions:

### Functional analysis
- Read `CHANGELOG.md` — what is already built? What MVP features exist and in what state?
- Read `src/app/App.tsx` — what screens/routes currently exist?
- Understand the current user journey where this feature fits: which screens lead to it? What does the user do before and after?
- Identify what pain point or gap in the current product this feature addresses.
- Check if a similar or adjacent flow already exists (e.g., if the feature is about "saving recipes", look at how recipe detail and the feed currently work from a product perspective).

### Technical analysis
- Scan `src/app/features/` — what feature folders exist? Which are structurally related to this one?
- Scan `src/app/components/` and `src/app/hooks/` — what shared primitives could be reused?
- Check `src/app/types/recipe.ts` — does the feature require new fields or types?
- Check `src/app/lib/api.ts` — what API calls exist? Will the feature need new endpoints?
- Look at tests in `src/tests/` that cover adjacent features — what's the test approach?
- Identify the state management pattern the feature should follow (TanStack Query vs Context).

**Document your findings** — both dimensions — before composing any questions.

---

## Phase 1: Requirements interview

After both analyses, send **one message** with targeted questions.
Use your findings from Phase 0 to skip generic questions the code already answers,
and to formulate specific ones that the code raises.

**Structure:**

```
Before writing the user story, I analyzed the codebase and found:
[2–3 bullet points of functional findings, e.g. "The recipe detail screen currently has no
save mechanism. The feed uses TanStack Query with mock data."]

[1–2 bullet points of technical findings, e.g. "The Recipe type has no 'saved' field yet.
There is no existing favorites hook or context."]

Based on this, I have some questions:

**Product and behavior**
1. [Functional Q derived from analysis — e.g. "Where should the save action live: on the
   card in the feed, on the recipe detail, or both?"]
2. What is the happy path step by step from the user's perspective?
3. What should happen if [specific edge case the code suggests — e.g. "the user is not
   logged in and tries to save"?]
4. Are there empty, loading, and error states to design?
5. What is explicitly OUT OF SCOPE for this ticket?

**Technical and integration**
6. [Technical Q derived from analysis — e.g. "Should saved recipes persist across sessions
   (needs backend) or just within the session (local state)?"]
7. Does this require a new API endpoint, or can we use an existing one?
8. Are there design mockups or references? Any animations or haptic feedback required?
9. Does this depend on another ticket being completed first?
```

Do NOT create the issue until answers are received.

---

## Phase 2: Confirm understanding

After receiving answers:
1. Summarize the feature in 2–3 sentences covering both product behavior and technical approach.
2. List the acceptance criteria (happy path, loading, empty, error, edge cases).
3. Flag any remaining ambiguities or assumptions you are making.
4. Ask: "Does this match your intent? Anything to correct before I create the issue?"

Do NOT proceed until the user confirms.

---

## Phase 3: Create the GitHub Issue

```markdown
## User Story
As a [user type], I want [goal] so that [benefit].

## Context
[1–2 sentences on the product gap and why this matters]

## Acceptance Criteria
- [ ] Happy path: [detailed description]
- [ ] Loading state: [description]
- [ ] Empty state: [description]
- [ ] Error state: [description]
- [ ] [Additional criteria from interview]

## Edge Cases
- [Each identified edge case]

## Out of Scope
- [Explicitly excluded items]

## Technical Notes
- Affected screens/features: [from functional analysis]
- Components/hooks to reuse: [from technical analysis]
- New components/hooks needed: [list]
- Type changes: [Recipe type additions, new interfaces]
- API changes: [new endpoints or none]
- State management: [TanStack Query / Context — which and why]
- Test files to create/update: [from technical analysis]
- Design constraints: [animations, haptic feedback, fonts, colors]
- Dependencies on other tickets: [list or "none"]
```

Use `--body-file` (see LAB_NOTES_POWERSHELL.md note #9):
```powershell
$env:PATH = "C:\Program Files\nodejs;" + $env:PATH
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
Set-Content -Path "issue-body.md" -Value $body -Encoding utf8
& "C:\Program Files\GitHub CLI\gh.exe" issue create --title "<title>" --body-file "issue-body.md" --repo canaledev/recipe-feeder-front-end
Remove-Item "issue-body.md"
```

---

## Phase 4: Board and branch

```powershell
& "C:\Program Files\GitHub CLI\gh.exe" project item-add 1 --owner canaledev --url <issue-url>
git checkout -b feature/<name>
```
Branch BEFORE any file is touched (see LAB_NOTES_GIT.md note #10).
Report the issue URL and branch name to the user.
