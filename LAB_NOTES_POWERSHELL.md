# Lab Notes: PowerShell & Windows Environment

Errors encountered in previous sessions. Rule + root cause. No narrative.
Load this file **before running any npm, node, or gh command**.

---

### 5. `npm`, `node`, and `gh` require the PowerShell tool, not Bash

**Cause:** All three are missing from bash PATH. The `&` operator in PowerShell breaks inside Bash. `$env:PATH` alone is not enough — ExecutionPolicy must also be unlocked.

**Rule:** For any command involving `npm`, `node`, or `gh`, always use the **PowerShell tool** with this preamble:
```powershell
$env:PATH = "C:\Program Files\nodejs;" + $env:PATH
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
& "C:\Program Files\GitHub CLI\gh.exe" <command>
```
**Warning sign:** `& "C:\..."` inside the Bash tool → move to PowerShell.

---

### 8. Unix shell equivalents do not exist in PowerShell

| Unix | PowerShell |
|---|---|
| `tail -N` | `Select-Object -Last N` |
| `head -N` | `Select-Object -First N` |
| `rm path\with\backslashes` | `Remove-Item "path\with\backslashes"` |

`rm` with Windows paths in the Bash tool corrupts the path (`\` is interpreted as escape). Always use PowerShell for Windows paths.

---

### 9. Multiline strings in PowerShell for git and gh

**Cause:** `$(cat <<'EOF'...)` is bash-only. PowerShell reserves `<<` without implementing it.

**Rules:**
- `git commit` multiline → PowerShell heredoc `@'...'@` (the closing `'@` must be at column 0):
```powershell
git commit -m @'
Message here.
Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
'@
```
- `gh pr create` with a long body → **always use `--body-file`**. Passing content via `$variable` also fails when the text contains `**`, `&`, or newlines (gh splits it into multiple arguments):
```powershell
Set-Content -Path "pr-body.md" -Value $body -Encoding utf8
& "C:\Program Files\GitHub CLI\gh.exe" pr create --body-file "pr-body.md"
Remove-Item "pr-body.md"
```
