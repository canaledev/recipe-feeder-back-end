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
