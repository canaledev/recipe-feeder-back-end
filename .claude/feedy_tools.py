"""
feedy_tools.py — MCP server for Feedy project Windows tooling.

Abstracts the PowerShell boilerplate required to run npm/node/gh on this Windows machine
and encapsulates the GitHub workflow (feature start, PR creation, lab note lookup).

Registered in .claude/settings.json as the "feedy-tools" mcpServer.
"""
import pathlib
import re
import subprocess
import tempfile
from typing import Optional

from mcp.server.fastmcp import FastMCP

NODEJS = r"C:\Program Files\nodejs"
GH = r"C:\Program Files\GitHub CLI\gh.exe"
# .claude/ is one level below project root
PROJECT_ROOT = pathlib.Path(__file__).parent.parent

# Note-to-file mapping
_BUILD_NOTES = {1, 2, 3, 4}
_PS_NOTES = {5, 8, 9}
_GIT_NOTES = {6, 7, 10}
_NOTE_FILES = {
    "build": PROJECT_ROOT / "LAB_NOTES_BUILD.md",
    "powershell": PROJECT_ROOT / "LAB_NOTES_POWERSHELL.md",
    "git": PROJECT_ROOT / "LAB_NOTES_GIT.md",
}

mcp = FastMCP("feedy-tools")


# ---------------------------------------------------------------------------
# Internal helpers
# ---------------------------------------------------------------------------

def _ps(script: str) -> tuple[str, int]:
    """Run a PowerShell snippet and return (output, returncode)."""
    result = subprocess.run(
        ["powershell.exe", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", script],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return (result.stdout + result.stderr).strip(), result.returncode


def _preamble() -> str:
    """PATH setup required before npm/gh commands."""
    return f'$env:PATH = "{NODEJS};" + $env:PATH; '


def _tmp_file(content: str) -> pathlib.Path:
    """Write content to a temp .md file in the project root and return its path."""
    fd, path = tempfile.mkstemp(suffix=".md", dir=str(PROJECT_ROOT))
    with open(fd, "w", encoding="utf-8") as f:
        f.write(content)
    return pathlib.Path(path)


# ---------------------------------------------------------------------------
# Tools
# ---------------------------------------------------------------------------

@mcp.tool()
def npm_run(cmd: str) -> str:
    """
    Run an npm script (e.g. 'test:run', 'build', 'lint') via PowerShell.
    Handles PATH and ExecutionPolicy automatically.
    Returns combined stdout+stderr.
    """
    script = _preamble() + f'& "{NODEJS}\\npm.cmd" run {cmd} 2>&1'
    output, code = _ps(script)
    return (f"[exit {code}]\n" if code != 0 else "") + output


@mcp.tool()
def gh_run(args: list[str]) -> str:
    """
    Run gh CLI with the given argument list.
    Example: gh_run(["issue", "list", "--repo", "canaledev/recipe-feeder-front-end"])
    Returns combined stdout+stderr.
    """
    escaped = " ".join(f'"{a}"' if (" " in a or any(c in a for c in "&|<>")) else a for a in args)
    script = _preamble() + f'& "{GH}" {escaped} 2>&1'
    output, code = _ps(script)
    return (f"[exit {code}]\n" if code != 0 else "") + output


@mcp.tool()
def start_feature(name: str, description: str) -> dict:
    """
    Full feature kickoff: create GitHub Issue, add to project board, create branch.

    Args:
        name: Feature name used as the issue title and branch name (e.g. 'user-auth').
        description: One-line feature description for the issue body.

    Returns dict with keys: issue_url, branch, error (None on success).
    """
    branch = "feature/" + re.sub(r"[^a-z0-9-]", "-", name.lower()).strip("-")

    body = (
        f"## User Story\n{description}\n\n"
        "## Acceptance Criteria\n- [ ] \n\n"
        "## Technical Notes\n- "
    )
    body_file = _tmp_file(body)
    try:
        out, code = _ps(
            _preamble()
            + f'& "{GH}" issue create --title "{name}" --body-file "{body_file}"'
            + ' --repo canaledev/recipe-feeder-front-end 2>&1'
        )
    finally:
        body_file.unlink(missing_ok=True)

    if code != 0:
        return {"issue_url": None, "branch": None, "error": f"Issue creation failed: {out}"}

    # gh issue create prints the issue URL on the last line
    issue_url = out.strip().splitlines()[-1].strip()

    # Add to project board
    board_out, code = _ps(
        _preamble() + f'& "{GH}" project item-add 1 --owner canaledev --url "{issue_url}" 2>&1'
    )
    if code != 0:
        return {"issue_url": issue_url, "branch": None, "error": f"Board add failed: {board_out}"}

    # Create branch
    branch_out, code = _ps(f'git -C "{PROJECT_ROOT}" checkout -b "{branch}" 2>&1')
    if code != 0:
        return {"issue_url": issue_url, "branch": None, "error": f"Branch failed: {branch_out}"}

    return {"issue_url": issue_url, "branch": branch, "error": None}


@mcp.tool()
def open_pr(title: str, body: str) -> str:
    """
    Create a PR on the current branch.
    Uses --body-file to avoid argument-splitting on special characters (lab note #9).
    Returns the PR URL or an error message.
    """
    safe_title = title.replace('"', '\\"')
    body_file = _tmp_file(body)
    try:
        out, code = _ps(
            _preamble()
            + f'& "{GH}" pr create --title "{safe_title}" --body-file "{body_file}" --base main 2>&1'
        )
    finally:
        body_file.unlink(missing_ok=True)

    return (f"[exit {code}]\n" if code != 0 else "") + out.strip()


@mcp.tool()
def get_lab_note(n: Optional[int] = None) -> str:
    """
    Return lab note(s) on demand.
      n=None  → index showing which file covers which note numbers
      n=0     → full content of all three files
      n=1–4   → note from LAB_NOTES_BUILD.md
      n=5,8,9 → note from LAB_NOTES_POWERSHELL.md
      n=6,7,10→ note from LAB_NOTES_GIT.md
    """
    if n is None:
        return (
            "Lab notes index:\n"
            "  LAB_NOTES_BUILD.md       notes 1-4    build/lint/test errors\n"
            "  LAB_NOTES_POWERSHELL.md  notes 5,8,9  npm/node/gh commands\n"
            "  LAB_NOTES_GIT.md         notes 6,7,10 git/GitHub workflow\n"
            "\nCall get_lab_note(0) to load all, or pass a specific note number."
        )

    if n == 0:
        parts = [p.read_text(encoding="utf-8") for p in _NOTE_FILES.values()]
        return "\n\n---\n\n".join(parts)

    if n in _BUILD_NOTES:
        path = _NOTE_FILES["build"]
    elif n in _PS_NOTES:
        path = _NOTE_FILES["powershell"]
    elif n in _GIT_NOTES:
        path = _NOTE_FILES["git"]
    else:
        return f"No note #{n}. Valid numbers: 1–4, 5, 6, 7, 8, 9, 10."

    content = path.read_text(encoding="utf-8")
    # Find the section starting with "### {n}." and extract until the next "### " or EOF
    match = re.search(rf"(### {n}\..+?)(?=\n### |\Z)", content, re.DOTALL)
    return match.group(1).strip() if match else content


if __name__ == "__main__":
    mcp.run()
