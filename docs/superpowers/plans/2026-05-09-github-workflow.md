# GitHub Workflow Skill — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a global `/github` Claude Code command that guides GitHub workflows (init, status, start, ship) using per-project state files to avoid context bloat and missed steps.

**Architecture:** One global markdown command at `~/.claude/commands/github.md` contains all workflow instructions. Per-project `.claude/github-config.md` holds static config; `.claude/github-session.md` holds dynamic session state. All multi-line `gh` output routes through `ctx_batch_execute`.

**Tech Stack:** Claude Code slash commands (markdown), `gh` CLI, `ctx_batch_execute` (context-mode), PowerShell / Bash, `.gitignore`

---

## File Map

| File | Action | Purpose |
|---|---|---|
| `C:\Users\canal\.claude\commands\github.md` | Create | Global command — all subcommand instructions |
| `{project}/.claude/github-config.md` | Create at runtime | Static per-project config (committed) |
| `{project}/.claude/github-session.md` | Create at runtime | Dynamic session state (gitignored) |
| `{project}/.gitignore` | Modify at runtime | Add `.claude/github-session.md` |

---

## Task 1: Create the global command file

**Files:**
- Create: `C:\Users\canal\.claude\commands\github.md`

- [ ] **Step 1: Write the command file**

Create `C:\Users\canal\.claude\commands\github.md` with this exact content:

```markdown
# GitHub Workflow

Use when: user types `/github`, `/github status`, `/github init`, `/github start #N`, `/github ship`, or asks to "start working on issue", "ship this", "open a PR", "show project status".

## State files

Before any subcommand, read these files from the current project's `.claude/` folder:
- **Config:** `.claude/github-config.md` — static project config (read-only during operations)
- **Session:** `.claude/github-session.md` — current working state (read and write)

If config file doesn't exist when needed, run `init` first.

---

## Context-mode rule

Route ALL `gh` commands that return lists or JSON through `ctx_batch_execute`. Use direct Bash only for:
- Single-item reads: `gh issue view N`, `gh pr view N`
- Write operations: `gh pr create`, `gh issue create`, `gh project item-edit`, `gh project item-add`
- `git` commands
- `dotnet build`, `dotnet test`

---

## File formats

### `.claude/github-config.md`

```
# GitHub Config
repo: owner/repo-name
project_number: 1
project_id: PVT_xxxxxxxxxxxx
board_inprogress_field_id: PVTSSF_xxxxxxxxxxxx
board_inprogress_option_id: xxxxxxxxxxxx
board_todo_field_id: PVTSSF_xxxxxxxxxxxx
board_todo_option_id: xxxxxxxxxxxx
```

### `.claude/github-session.md`

```
# GitHub Session
issue_number: 
issue_title: 
branch: 
pr_number: 
pr_url: 
workflow_step: 
```

`workflow_step` values: `started` | `pr_open`

---

## Subcommands

### init

First-time setup for a new project.

1. Run via `ctx_batch_execute`:
   - label `"repo info"`: `gh repo view --json nameWithOwner`
   - label `"projects"`: `gh project list --owner @me --format json --limit 10`
2. If multiple projects found, ask user which `project_number` applies to this repo
3. Run via `ctx_batch_execute`:
   - label `"board fields"`: `gh project field-list <project_number> --owner <owner> --format json`
4. Find the "Status" field. Extract:
   - `board_inprogress_field_id`: the field's `id`
   - `board_inprogress_option_id`: the option named "In Progress" `id`
   - `board_todo_field_id`: same field `id` (Status field is shared)
   - `board_todo_option_id`: the option named "Todo" `id`
5. Write `.claude/github-config.md` with all values
6. Check `.gitignore` — append `.claude/github-session.md` if not already present
7. Write empty `.claude/github-session.md` with blank fields
8. Report: "GitHub workflow initialized for `<repo>`. Config at `.claude/github-config.md`."

---

### status

Aggregated project snapshot.

1. Read `.claude/github-config.md`
2. Read `.claude/github-session.md`
3. Run via `ctx_batch_execute`:
   - label `"my open issues"`: `gh issue list --state open --assignee @me --json number,title,labels --limit 20`
   - label `"open PRs"`: `gh pr list --state open --json number,title,headRefName,isDraft --limit 10`
   - label `"board in progress"`: `gh project item-list <project_number> --owner <owner> --format json --limit 20`
4. Present concise summary:

```
## GitHub Status — <repo>

### In Progress (board)
- <item titles>

### My Open Issues
- #N title [label]

### Open PRs
- #N title (branch) [draft?]

### Current Session
issue: #<issue_number> — <issue_title>
branch: <branch>
step: <workflow_step>
```

---

### start #N

Begin work on issue N.

1. Read `.claude/github-config.md`
2. Run: `gh issue view N --json number,title,body,labels`
3. Derive branch name:
   - prefix: `fix/` if any label is `bug`, else `feat/`
   - slug: title → lowercase → replace spaces with `-` → remove non-alphanumeric except `-` → truncate to 40 chars
   - result: `feat/N-slug` or `fix/N-slug`
4. Check if branch exists: `git branch --list feat/N-* fix/N-*`
   - If exists: `git checkout <branch>`
   - If not: `git checkout -b <branch>`
5. Get item ID for this issue on the project board via `ctx_batch_execute`:
   - label `"board items"`: `gh project item-list <project_number> --owner <owner> --format json --limit 50`
   - Find item where `content.number == N`, extract item `id`
6. Move to In Progress:
   ```
   gh project item-edit --project-id <project_id> --id <item_id> --field-id <board_inprogress_field_id> --single-select-option-id <board_inprogress_option_id>
   ```
7. Write `.claude/github-session.md`:
   ```
   # GitHub Session
   issue_number: N
   issue_title: <title>
   branch: <branch>
   pr_number: 
   pr_url: 
   workflow_step: started
   ```
8. Report: "Working on #N — `<branch>` checked out. Issue moved to In Progress."

---

### ship

Complete work and open PR.

1. Read `.claude/github-config.md` and `.claude/github-session.md`
   - If `issue_number` is empty, ask: "Which issue does this work relate to? (provide #N)"
2. Verify build:
   ```
   dotnet build
   ```
   Stop if build fails. Fix errors first.
3. Verify tests:
   ```
   dotnet test
   ```
   Stop if tests fail. Fix failures first.
4. Show `git diff --stat origin/master...HEAD` — summarize changed files
5. Check `CHANGELOG.md` — read `[Unreleased]` section. If it does not reflect current work, update it now before committing.
6. Stage specific files (never `git add -A`):
   ```
   git add <list of modified files>
   ```
7. Commit using PowerShell here-string to handle multi-line message:
   ```powershell
   git commit -m @'
   feat: <summary in imperative present tense>

   Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>
   '@
   ```
   Use `fix:` prefix if the issue label is `bug`.
8. Push:
   ```
   git push -u origin <branch>
   ```
9. Build PR body — analyze diff to write specific summary bullets:
   ```
   ## Summary
   - <bullet derived from diff>
   - <bullet derived from diff>

   ## Test plan
   - [x] `dotnet build` passes
   - [x] `dotnet test` passes
   - [ ] <manual test if feature has UI or auth flow>

   Fixes #<issue_number>

   🤖 Generated with [Claude Code](https://claude.com/claude-code)
   ```
10. Create PR:
    ```powershell
    gh pr create --title "<issue_title>" --body @'
    <body>
    '@
    ```
11. Extract PR URL from output
12. Add PR to project board:
    ```
    gh project item-add <project_number> --owner <owner> --url <pr_url>
    ```
13. Update `.claude/github-session.md` — set `pr_number`, `pr_url`, `workflow_step: pr_open`
14. Report to user:
    > PR #N opened: `<pr_url>`
    >
    > Before merging, verify manually:
    > - [ ] <each unchecked item from the test plan>
```

- [ ] **Step 2: Verify file exists and is valid markdown**

Run:
```powershell
Test-Path "C:\Users\canal\.claude\commands\github.md"
```
Expected output: `True`

- [ ] **Step 3: Verify Claude Code sees the command**

In Claude Code terminal, type `/github` — it should autocomplete or show the command is available.  
If not, restart Claude Code to pick up the new command file.

- [ ] **Step 4: Commit the command file is global — no commit needed for it**

The global command lives outside any repo. No commit. Proceed to Task 2.

---

## Task 2: Initialize the skill in this project

**Files:**
- Create: `.claude/github-config.md` (generated by running `/github init`)
- Modify: `.gitignore` (append session file)
- Create: `.claude/github-session.md` (empty, gitignored)

- [ ] **Step 1: Ensure `.claude/` directory exists**

```powershell
New-Item -ItemType Directory -Force -Path ".claude" | Out-Null
```

- [ ] **Step 2: Run `/github init`**

Invoke `/github init` in this session. The command will:
- Query `gh repo view` and `gh project list` via ctx_batch_execute
- Query `gh project field-list` for board column IDs
- Write `.claude/github-config.md`
- Append `.claude/github-session.md` to `.gitignore`
- Create empty `.claude/github-session.md`

- [ ] **Step 3: Verify config file was created**

Read `.claude/github-config.md` — confirm it has `repo`, `project_id`, `board_inprogress_field_id`, and `board_inprogress_option_id` populated (not placeholder values).

- [ ] **Step 4: Verify gitignore entry**

Run:
```powershell
Select-String -Path ".gitignore" -Pattern "github-session"
```
Expected: a matching line containing `.claude/github-session.md`

- [ ] **Step 5: Commit the config file**

```powershell
git add .claude/github-config.md .gitignore
git commit -m @'
chore: add GitHub workflow config for /github skill

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>
'@
```

---

## Task 3: Commit spec and plan

**Files:**
- Commit: `docs/superpowers/specs/2026-05-09-github-workflow-design.md`
- Commit: `docs/superpowers/plans/2026-05-09-github-workflow.md`

- [ ] **Step 1: Stage and commit docs**

```powershell
git add docs/superpowers/specs/2026-05-09-github-workflow-design.md
git add docs/superpowers/plans/2026-05-09-github-workflow.md
git commit -m @'
docs: add GitHub workflow skill spec and implementation plan

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>
'@
```

- [ ] **Step 2: Verify git log**

```powershell
git log --oneline -3
```
Expected: two new commits visible at the top.

---

## Self-review

**Spec coverage check:**
- `init` subcommand → Task 2 ✓
- `status` subcommand → inside command file (Task 1) ✓
- `start #N` subcommand → inside command file (Task 1) ✓
- `ship` subcommand → inside command file (Task 1) ✓
- Per-project config file → Task 2 ✓
- Gitignore for session file → Task 2 ✓
- Context-mode integration → documented in command file ✓
- Global placement → `~/.claude/commands/` ✓

**No placeholders:** All steps have exact commands, file paths, and expected outputs. ✓

**Type consistency:** File format in the command file matches examples in spec. ✓
