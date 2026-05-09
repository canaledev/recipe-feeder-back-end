# Post-Mortem / Lab Notes Update

Capture lessons from errors in the current session and write them to the appropriate Lab Notes file.
Run this command after identifying a class of recurring or avoidable errors.

---

## Step 1: Identify the domain

Determine which lab notes file owns this class of error:

| Domain | File |
|---|---|
| TypeScript, ESLint, Vitest, build | `LAB_NOTES_BUILD.md` |
| PowerShell, Windows paths, npm/gh CLI | `LAB_NOTES_POWERSHELL.md` |
| git staging, PRs, GitHub project board | `LAB_NOTES_GIT.md` |
| react-i18next, locale files, translated tests | `LAB_NOTES_I18N.md` |
| New domain | Create `LAB_NOTES_<DOMAIN>.md` |

If the error spans multiple domains, split it — one note per domain file.

---

## Step 2: Extract the invariant, not the incident

For each error, ask:
- **What class of problem does this represent?** (not: what went wrong this time)
- **What invariant, if always enforced, would have prevented it?**
- **Does the rule cover only this case, or a broader pattern?**

A good rule prevents a family of errors. A bad rule prevents only the one that just happened.

---

## Step 3: Write the note

Each note follows this structure — no deviation:

```markdown
### N. Title that names the invariant, not the symptom

**Cause:** One or two sentences. Why does this class of problem occur?
What assumption fails, what boundary is crossed, what dependency is hidden?

**Rule:** The invariant to enforce going forward. Broad enough to cover similar cases.
Include a code snippet only if the correct pattern is non-obvious.
```

Quality criteria:
- Title: names the rule, not the incident ("Feature branch must exist before first file is touched" not "Don't code on main")
- Cause: explains the mechanism, not just the event
- Rule: states what must always be true, not just what to do this time
- No narrative, no "we did X and it failed" — the format is a reference, not a story

---

## Step 4: Check for existing notes that can absorb this one

Before adding a new note, read the target file. Ask: does an existing rule already cover this? If so, extend it rather than duplicate. Two notes that share a cause belong together.

---

## Step 5: Write or update the file

Add the new note to the bottom of the appropriate file. If a new file is created:
1. Follow the header format of existing lab notes files
2. Add it to `.claude/commands/lab.md` reference table
3. Add it to `CLAUDE.md` under "Lab notes by domain"

---

## Step 6: Commit

```powershell
$env:PATH = "C:\Program Files\nodejs;" + $env:PATH
git add LAB_NOTES_<DOMAIN>.md
git commit -m @'
docs: update LAB_NOTES_<DOMAIN>.md with post-mortem from <session topic>

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```
