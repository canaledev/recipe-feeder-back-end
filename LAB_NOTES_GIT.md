# Lab Notes: Git & GitHub Workflow

Errors encountered in previous sessions. Rule + root cause. No narrative.
Load this file **before any git commit, PR creation, or GitHub project board operation**.

---

### 6. Verify `git status` before opening a PR and after any commit error

**Combined causes:**
- New files are left unstaged if `git add` is forgotten
- A PowerShell parse error aborts the entire block, including the `git add` that preceded the failed `git commit`. Staging does not happen even if `git add` appears first in the script

**Rule:** Before opening a PR: run `git status` to confirm no uncommitted changes. If a commit command fails, assume `git add` also did not run and repeat it. `package-lock.json` always goes together with changes to `package.json`.

---

### 7. `gh project item-add` requires the `project` scope in the token

**Cause:** `gh auth login` does not request the `project` scope by default. Without it, `gh project item-add` fails with `missing required scopes [read:project]`.

**Rule:** First time using `gh project` commands, verify and refresh (always with `-h github.com` — without it the command fails in non-interactive environments):
```powershell
& "C:\Program Files\GitHub CLI\gh.exe" auth status
& "C:\Program Files\GitHub CLI\gh.exe" auth refresh -h github.com -s project
```
The command launches device flow — show the code to the user and run with `run_in_background: true`.

---

### 10. Create the feature branch BEFORE implementing, not after

**What happened:** The entire feature was implemented on `main` and the feature branch was created at the end with `git checkout -b`. Staging and history stayed on `main` throughout development, violating the no-direct-push-to-main rule and risking an accidental push.

**Rule:** The first command of any development task is:
```powershell
git checkout -b feature/<name>
```
Before touching a single file. If the error is caught late, `git checkout -b feature/<name>` from `main` carries uncommitted changes to the new branch — but that is not an excuse for not doing it first.
