# Ship

Run the full pre-PR checklist for the current feature branch.
Load `LAB_NOTES_GIT.md` and `LAB_NOTES_POWERSHELL.md` before running any command.

**Do not proceed if any step fails — report the issue and stop.**

---

## Step 1: Clean the kitchen

Scan `git diff master...HEAD` for debris left in the branch:
- `Console.Write`, `Console.WriteLine`, debugger/breakpoint statements (flag any that look unintentional)
- Large blocks of commented-out code (not docstrings or intentional inline comments)
- Temporary files: `pr-body.md`, `*.tmp`, `*.bak`
- Unresolved TODOs or FIXMEs *added in this branch* (existing ones are not your problem)

Report all findings and ask the user to confirm before continuing.

---

## Step 2: Run tests

```powershell
$env:PATH = "C:\Program Files\dotnet;" + $env:PATH
dotnet test 2>&1
```
Stop and report if any test fails.

---

## Step 3: Run build

```powershell
$env:PATH = "C:\Program Files\dotnet;" + $env:PATH
dotnet build 2>&1 | Select-Object -Last 10
```
Stop and report if the build fails.

---

## Step 4: Verify smoke tests are updated

This step is **mandatory**. Every endpoint added or modified in this branch must have a corresponding curl test updated or created.

1. List the endpoints touched in this branch:
```powershell
git diff master...HEAD -- src/Feedy.Api/Controllers/
```

2. For each changed endpoint, confirm that a curl test for it exists or was updated in this session.
   - If yes → continue.
   - If no → **stop**. Run the `docker-api-smoke-test` skill to add the missing tests before proceeding.

3. Run the smoke test suite:
   - Use the `docker-api-smoke-test` skill.
   - Document results (status codes + response bodies) for inclusion in the PR test plan.

---

## Step 5: Verify CHANGELOG

```powershell
git diff master -- CHANGELOG.md
```
If the output is empty, **stop and warn the user** — updating `CHANGELOG.md` in the `[Unreleased]` section is mandatory before any PR.

---

## Step 6: Confirm staging is clean

```powershell
git status
```
If there are uncommitted changes, remind the user to stage and commit them first (see LAB_NOTES_GIT.md note #6).

---

## Step 7: Collect PR metadata

Ask the user for:
- PR title (under 70 characters)
- 2–3 bullet points summarizing what changed and why

---

## Step 8: Create the PR

Use `--body-file` to avoid argument-splitting issues with special characters (see LAB_NOTES_POWERSHELL.md note #9):

```powershell
$body = @"
## Summary
- <bullet 1>
- <bullet 2>

## Test plan
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
- [ ] Smoke tests run via Docker (results below)

### Smoke test results
| Endpoint | Expected | Actual |
|---|---|---|
| GET /health | 200 | ✅ 200 |
| <endpoint> | <code> | <result> |

🤖 Generated with [Claude Code](https://claude.com/claude-code)
"@
Set-Content -Path "pr-body.md" -Value $body -Encoding utf8
& "C:\Program Files\GitHub CLI\gh.exe" pr create --title "<title>" --body-file "pr-body.md" --base master
Remove-Item "pr-body.md"
```

Return the PR URL to the user.
