# GitHub Workflow Skill — Design Spec

**Date:** 2026-05-09  
**Author:** Claude (Haiku 4.5) + canaledev  
**Status:** Approved

---

## Problem

Working with GitHub from within Claude Code involves repeated friction:
- Multi-step workflows (create issue → branch → work → PR → board) are error-prone and incomplete
- Raw `gh` output bloats the context window with JSON noise
- Session state (current issue, branch, PR) must be re-queried every time
- Static config (project ID, board column IDs) is re-fetched even though it never changes
- PR descriptions, commit messages, and CHANGELOG updates are assembled from scratch each time

---

## Solution

A **global Claude Code slash command** (`/github`) that guides my behavior through GitHub workflows using:
1. Per-project config files (static, committed)
2. Per-project session state files (dynamic, gitignored)
3. `ctx_batch_execute` for GitHub queries — processed output, no raw JSON in context

---

## Architecture

```
~/.claude/commands/github.md              ← global skill (one file, all projects)

{project}/.claude/github-config.md        ← static config per project (committed)
{project}/.claude/github-session.md       ← session state per project (gitignored)
```

### Global skill (`~/.claude/commands/github.md`)
- Markdown file with workflow instructions for Claude
- Subcommands: `init`, `status`, `start`, `ship`
- Knows to look for config/session files in `.claude/` of the current working directory
- Instructs Claude to use `ctx_batch_execute` for all multi-line `gh` output

### Per-project config (`.claude/github-config.md`)
Created once via `/github init`. Contains:
```markdown
# GitHub Config
repo: owner/repo-name
project_number: 1
project_id: PVT_xxxxxxxxxxxx
board_inprogress_field_id: PVTSSF_xxxxxxxxxxxx
board_inprogress_option_id: xxxxxxxxxxxx
board_todo_field_id: PVTSSF_xxxxxxxxxxxx
board_todo_option_id: xxxxxxxxxxxx
```

### Per-project session state (`.claude/github-session.md`)
Written/updated during workflows. Contains:
```markdown
# GitHub Session
issue_number: 42
issue_title: Add smart feed ranking
branch: feat/42-smart-feed
pr_number: 18
pr_url: https://github.com/owner/repo/pull/18
workflow_step: pr_open
```

---

## Subcommands

### `/github init`
**Purpose:** First-time setup for a new project.  
**Steps:**
1. Query `gh repo view` and `gh project list` to get repo + project metadata
2. Query board field/column IDs via `gh project field-list`
3. Write `.claude/github-config.md`
4. Add `.claude/github-session.md` to `.gitignore`

### `/github status`
**Purpose:** Aggregated project snapshot without context bloat.  
**Steps (via ctx_batch_execute):**
1. Open issues assigned to current user
2. Open PRs on this repo
3. Project board — In Progress items
4. Latest failing CI checks (if any)
**Output:** Concise markdown summary in context (not raw JSON)

### `/github start #N`
**Purpose:** Begin work on an issue cleanly.  
**Steps:**
1. Read `.claude/github-config.md`
2. Fetch issue #N title + body via `gh issue view`
3. Derive branch name: `feat/N-slug` or `fix/N-slug` where slug = issue title lowercased, spaces→hyphens, max 40 chars, no special chars
4. Create and checkout branch
5. Move issue to "In Progress" on project board via `gh project item-edit`
6. Write `.claude/github-session.md` with issue_number, branch, workflow_step: started

### `/github ship`
**Purpose:** Complete work and open PR with all project conventions satisfied.  
**Preconditions:** Read `.claude/github-session.md` to restore context.  
**Steps (with verification at each):**
1. Read session state — confirm branch and issue number
2. Verify `dotnet build` passes
3. Verify `dotnet test` passes
4. Update `CHANGELOG.md` `[Unreleased]` section
5. `git add` staged changes
6. Commit with co-authored-by footer
7. `git push -u origin <branch>`
8. Create PR via `gh pr create` with body template:
   ```
   ## Summary
   - <bullets from diff analysis>

   ## Test plan
   - [ ] dotnet build ✓
   - [ ] dotnet test ✓
   - [ ] <manual tests if UI involved>

   Fixes #<issue_number>

   🤖 Generated with Claude Code
   ```
9. Add PR to project board via `gh project item-add`
10. Update `.claude/github-session.md` with pr_number, pr_url, workflow_step: pr_open
11. Report to user: PR URL + any pending manual tests

---

## Context-Mode Integration

All `gh` commands that produce multi-line output MUST be routed through `ctx_batch_execute`:
- `gh issue list`, `gh pr list`, `gh project item-list` → context-mode
- `gh repo view`, `gh issue view #N` → can be direct if single item
- `gh project field-list` → context-mode (JSON heavy)

Single-result queries (e.g., `gh pr create`, `gh issue create`) can run via Bash directly.

---

## File Ownership & Gitignore

`.claude/github-config.md` — committed, shared with team  
`.claude/github-session.md` — gitignored, local only

The `init` subcommand appends to `.gitignore` automatically:
```
.claude/github-session.md
```

---

## Out of Scope (MVP)

- Webhook / CI monitoring
- GitHub Actions management
- Label/milestone management
- Multi-repo workflows
