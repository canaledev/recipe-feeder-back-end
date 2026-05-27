# Next Task

Analyze the backend project's open issues and recommend the best next task to work on, ranked by fewest inter-issue dependencies.

Read `.claude/github-config.md` before running any command. Use the `repo` value for all `gh` calls.

---

## Step 1: Load graph and fetch open issue titles

**1a. Load cached dependency graph:**

Read `.claude/issue-dependency-graph.json`.
If the file does not exist, skip to **Cache Miss Fallback** at the bottom.

**1b. Fetch open issues (titles only — no body needed):**

```powershell
gh issue list --repo <repo> --state open --json number,title --limit 50
```

**1c. Reconcile:**

- `open_numbers` = set of issue numbers from the `gh` output.
- `active_issues` = graph entries whose number is in `open_numbers`.
- For each open number **not** in the graph, emit a warning inline in the table:
  `⚠️ #N (<title>) — not in dependency graph. Run /start-feature to register it.`
- Issues in the graph but absent from `open_numbers` are closed — exclude from analysis.

---

## Step 2: Compute metrics

For each issue in `active_issues` (skip entries with `"is_epic": true`):

- `dep_count` = count of `depends_on` entries whose number is **also in `open_numbers`**
  (a dependency that is already closed does not block).
- `blocking_count` = count of other active issues that list this issue in their `depends_on`.
- `blocked_by` = `depends_on` entries that are still open.

---

## Step 3: Present the dependency table

```
## Open Issues — Dependency Analysis

| Priority | # | Title | Status | Blocks |
|----------|---|-------|--------|--------|
| 1 | #N | Short title | ✅ Ready | #X, #Y |
| 2 | #M | Short title | ⛔ Blocked by #N | — |
```

Sort rows by:
1. `dep_count` ASC — zero-blocker issues first.
2. `blocking_count` DESC — prefer the issue that unblocks the most others.
3. Issue number ASC — tiebreaker.

Status column:
- `✅ Ready` — dep_count == 0
- `⛔ Blocked by #N, #M` — list the open blockers by number

Note below the table the graph's `updated_at` date so the user knows when it was last rebuilt.

---

## Step 4: Recommend

```
## Recommendation

**Work on #N — <Title> next.**

<Why: zero blockers + what it unblocks. Any caveat (stale graph warning, issue may already be done, etc.).>
```

---

## Step 5: Offer to start

Ask the user:

> "Do you want to start working on #N? I'll invoke the `github` skill (`/github start #N`) to create the branch and move the issue to In Progress on the board."

If the user confirms, run `/github start #N` following the `github` skill's `start` subcommand exactly.

---

## Cache Miss Fallback

If `.claude/issue-dependency-graph.json` is missing:

> ⚠️ Dependency graph not found — running full body analysis to rebuild it. This uses significantly more tokens than the cached path.

1. Fetch all open issues with full bodies:
   ```powershell
   gh issue list --repo <repo> --state open --json number,title,body,labels --limit 50
   ```
2. Analyze dependencies for each issue:
   - Explicit: body says "Depends on #N", "Blocked by #N", "Requires #N"
   - Structural: issue adds auth endpoints and auth is not yet merged
   - Structural: issue extends an entity/table/controller introduced by another open issue
   - NOT a dependency: frontend cross-references, soft recommendations
3. Present the dependency table (Steps 3–5 above).
4. Write the rebuilt graph to `.claude/issue-dependency-graph.json`:
   ```json
   {
     "version": 1,
     "updated_at": "<today ISO date>",
     "repo": "<repo>",
     "note": "Auto-maintained. Updated by /start-feature after issue creation. Rebuild by deleting this file and running /next-task.",
     "issues": {
       "<number>": {
         "title": "<title>",
         "depends_on": [<number>, ...],
         "rationale": "<one line: why this dependency exists>",
         "is_epic": false
       }
     }
   }
   ```
   Use the Write tool — do not use Bash or shell redirection to write the file.
