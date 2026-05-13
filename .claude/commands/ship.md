# Ship

Run the full pre-PR checklist for the current feature branch.
Load `LAB_NOTES_GIT.md` and `LAB_NOTES_POWERSHELL.md` before running any command.

**Do not proceed if any step fails — report the issue and stop.**

---

## Step 1: Clean the kitchen

Scan `git diff main...HEAD` for debris left in the branch:
- `console.log`, `console.error`, `debugger` statements (flag any that look unintentional)
- Large blocks of commented-out code (not docstrings or intentional inline comments)
- Temporary files: `pr-body.md`, `*.tmp`, `*.bak`
- Unresolved TODOs or FIXMEs *added in this branch* (existing ones are not your problem)

Report all findings and ask the user to confirm before continuing.

---

## Step 2: Run tests

```powershell
$env:PATH = "C:\Program Files\nodejs;" + $env:PATH
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
& "C:\Program Files\nodejs\npm.cmd" run test:run 2>&1
```
Stop and report if any test fails.

---

## Step 3: Run build

```powershell
& "C:\Program Files\nodejs\npm.cmd" run build 2>&1
```
Stop and report if the build fails (TypeScript errors count).

---

## Step 4: Verify CHANGELOG

```powershell
git diff main -- CHANGELOG.md
```
If the output is empty, **stop and warn the user** — updating `CHANGELOG.md` in the `[Unreleased]` section is mandatory before any PR.

---

## Step 5: Confirm staging is clean

```powershell
git status
```
If there are uncommitted changes, remind the user to stage and commit them first (see LAB_NOTES_GIT.md note #6).

---

## Step 6: Collect PR metadata

Ask the user for:
- PR title (under 70 characters)
- 2–3 bullet points summarizing what changed and why

---

## Step 7: Create the PR

Use `--body-file` to avoid argument-splitting issues with special characters (see LAB_NOTES_POWERSHELL.md note #9):

```powershell
$body = @"
## Summary
- <bullet 1>
- <bullet 2>

## Test plan
- [ ] All tests pass (`npm run test:run`)
- [ ] Production build succeeds (`npm run build`)
- [ ] Manual UI verification (list affected screens)

🤖 Generated with [Claude Code](https://claude.com/claude-code)
"@
Set-Content -Path "pr-body.md" -Value $body -Encoding utf8
& "C:\Program Files\GitHub CLI\gh.exe" pr create --title "<title>" --body-file "pr-body.md" --base main
Remove-Item "pr-body.md"
```

Return the PR URL to the user.
