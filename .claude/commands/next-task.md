# Next Task

Analyze the backend project's open issues and recommend the best next task to work on, ranked by fewest inter-issue dependencies.

Read `.claude/github-config.md` before running any command. Use the `repo` value for all `gh` calls.

---

## Step 1: Fetch all open issues

```powershell
& "C:\Program Files\GitHub CLI\gh.exe" issue list --repo <repo> --state open --json number,title,body,labels --limit 50
```

Read the full body of every issue returned — the dependency analysis depends on the content, not just the title.

---

## Step 2: Analyze dependencies (use technical judgment)

For each open issue, determine its **direct dependencies**: other open issues that must be done first because the feature literally cannot function without them.

An issue B depends on issue A when:
- The body explicitly says so ("Depends on #A", "Blocked by #A", "Requires #A")
- B adds authenticated endpoints, and A implements the auth/JWT infrastructure
- B extends an entity, endpoint, or database table introduced by A
- B's acceptance criteria reference state that A is responsible for creating

**What is NOT a dependency:**
- Frontend cross-references (`canaledev/recipe-feeder-front-end#N`) — these are context, not blockers
- Issues that would be nice to do first but don't technically block anything
- Shared domain entities that can coexist in parallel branches

Build this output for each issue:
- `depends_on[]`: numbers of open issues this issue cannot start without
- `blocks[]`: numbers of open issues that list this issue as a dependency

---

## Step 3: Present the dependency table

```
## Open Issues — Dependency Analysis

| Priority | # | Title | Status | Blocks |
|----------|---|-------|--------|--------|
| 1 | #N | Short title | ✅ Ready | #X, #Y |
| 2 | #M | Short title | ⛔ Blocked by #N | — |
| ...
```

Sort rows by:
1. `dep_count` ASC — issues with no open blockers come first
2. `blocking_count` DESC — among equally unblocked issues, prefer the one that unblocks the most others
3. Issue number ASC — tiebreaker: lower number was opened earlier

Status column:
- `✅ Ready` — dep_count == 0
- `⛔ Blocked by #N, #M` — list which open issues must close first

---

## Step 4: Recommend

After the table, give a clear recommendation in 2–3 sentences:

```
## Recommendation

**Work on #N — <Title> next.**

<Why this one: zero blockers + what it unblocks. Any notable risk or caveat.>
```

---

## Step 5: Offer to start

Ask the user:

> "Do you want to start working on #N? I'll invoke the `github` skill (`/github start #N`) to create the branch and move the issue to In Progress on the board."

If the user confirms, run `/github start #N` following the `github` skill's `start` subcommand exactly.
