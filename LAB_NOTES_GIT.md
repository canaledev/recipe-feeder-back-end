# Lab Notes: Git & GitHub Workflow

Errors encountered in previous sessions. Rule + root cause. No narrative.

> **Note:** The `/github` skill (global command) encodes all three invariants below into its `start` and `ship` workflows. These notes explain the *why* — load them when debugging workflow failures.

---

### 6. PowerShell parse error aborts `git add` + `git commit` block atomically

**Cause:** A PowerShell syntax error in a compound block aborts the entire block, including `git add` that preceded the failed `git commit`. Staging never happens even if `git add` appears first.

**Why it matters:** Encoded in `/github ship` steps 4 and 7 — `git status` before staging and after staging to catch this.

---

### 7. `gh project` commands require the `project` scope — not granted by default

**Cause:** `gh auth login` does not request `project` scope. Without it, all `gh project item-*` commands fail with `missing required scopes [read:project]`.

**Fix** (always with `-h github.com` — omitting it fails in non-interactive environments):
```powershell
& "C:\Program Files\GitHub CLI\gh.exe" auth refresh -h github.com -s project
```
Run with `run_in_background: true` — launches device flow, show code to user.

**Why it matters:** Encoded in `/github init` prerequisite check.

---

### 10. Feature branch must be created before touching any file

**Cause:** Implementing on `main` then branching late keeps history on `main`, violating the no-direct-push rule.

**Why it matters:** Encoded in `/github start` — branch creation is step 4, before any file changes.

---

### 11. Feature branch must be created from a fresh fetch of master, not from stale HEAD

**Cause:** `git checkout -b feature/x` branches from wherever HEAD currently points. If local master is behind origin/master, the branch diverges immediately and will have merge conflicts on the PR — even though no conflicting work was done.

**Rule:** Always fetch before branching:
```bash
git fetch origin
git checkout -b feature/x origin/master
```
Never branch from local master without verifying it matches `origin/master` first (`git log master..origin/master` — must be empty).

---

### 12. `git rebase` requires a clean working tree — stash unstaged changes first

**Cause:** `git rebase origin/master` aborts with "You have unstaged changes" if any tracked file is modified but not staged. The rebase does not run at all; no commits are applied.

**Rule:** Before any rebase, always stash or commit outstanding changes:
```bash
git stash
git rebase origin/master
git stash pop
```

---

### 13. CHANGELOG edits must preserve the markdown element type of surrounding content

**Cause:** When inserting new bullet entries into CHANGELOG.md using the Edit tool, the `new_string` can accidentally change a list item (`- text`) into a heading (`### text`) if the replacement boundary falls at the start of the surrounding content. The file builds and tests pass — the corruption is visual-only and easy to miss.

**Rule:** After any CHANGELOG edit, read the modified section back and verify every line that was previously a `- ` bullet is still a `- ` bullet. Never include a bare `###` line in `new_string` unless explicitly adding a new section header.
