#Requires -Version 5.1
# Incremental database migration runner for feedy-postgres.
#
# Usage:  .\db\migrate.ps1
#
# 1. Starts the postgres container (docker-compose up -d postgres).
# 2. Waits up to 30 s for it to accept connections.
# 3. Bootstraps the schema_migrations tracking table (idempotent).
# 4. Applies every script in db/infrastructure/ then db/data/ that has
#    not yet been recorded in schema_migrations.
# 5. Prints [SKIP] for already-applied scripts, [OK] for newly applied ones.
#
# Re-running is always safe: already-applied scripts are never run twice.

$ErrorActionPreference = 'Stop'

# Docker Desktop does not add itself to the system PATH in all environments.
$env:PATH = 'C:\Program Files\Docker\Docker\resources\bin;' + $env:PATH

$Container  = 'feedy-postgres'
$DbName     = 'feedy_dev'
$DbUser     = 'postgres'
$ScriptRoot = $PSScriptRoot   # resolves to the db/ folder

# -- 1. Start postgres container ----------------------------------------------
Write-Host 'Starting postgres container...'
Push-Location "$ScriptRoot\.."
try {
    docker compose up -d postgres
} finally {
    Pop-Location
}

# -- 2. Wait for postgres to accept connections (max 30 s) --------------------
Write-Host 'Waiting for postgres to be ready...'
$deadline = (Get-Date).AddSeconds(30)
$ready    = $false
while ((Get-Date) -lt $deadline) {
    docker exec $Container pg_isready -U $DbUser | Out-Null
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 2
}
if (-not $ready) {
    Write-Error 'Postgres did not become ready within 30 seconds.'
    exit 1
}
Write-Host 'Postgres is ready.'

# -- 3. Bootstrap schema_migrations table -------------------------------------
# CREATE TABLE IF NOT EXISTS is a no-op when the table already exists.
# This is the only DDL statement that runs on every invocation.
$bootstrapSql = 'CREATE TABLE IF NOT EXISTS schema_migrations (script_name VARCHAR(255) PRIMARY KEY, applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW());'
docker exec $Container psql -U $DbUser -d $DbName -c $bootstrapSql | Out-Null

# -- 4. Collect scripts: infrastructure/ first (schema), then data/ (seed) ---
$scripts  = @()
$scripts += Get-ChildItem "$ScriptRoot\infrastructure\*.sql" | Sort-Object Name |
            ForEach-Object { 'infrastructure/' + $_.Name }
$scripts += Get-ChildItem "$ScriptRoot\data\*.sql" | Sort-Object Name |
            ForEach-Object { 'data/' + $_.Name }

$applied = 0
$skipped = 0

Write-Host ''

foreach ($key in $scripts) {
    $fullPath = Join-Path $ScriptRoot ($key -replace '/', '\')

    # Check whether this script has already been applied
    $checkSql = "SELECT 1 FROM schema_migrations WHERE script_name = '" + $key + "';"
    $exists   = docker exec $Container psql -U $DbUser -d $DbName -tAc $checkSql | Out-String

    if ($exists.Trim() -eq '1') {
        Write-Host ('[SKIP] ' + $key)
        $skipped++
        continue
    }

    # Copy script into the container and execute it
    docker cp $fullPath ($Container + ':/tmp/migration.sql')
    docker exec $Container psql -U $DbUser -d $DbName -f /tmp/migration.sql

    if ($LASTEXITCODE -ne 0) {
        Write-Error ('[ERROR] ' + $key + ' - psql exited with code ' + $LASTEXITCODE + '. Migration halted.')
        exit 1
    }

    # Record as applied
    $insertSql = "INSERT INTO schema_migrations (script_name) VALUES ('" + $key + "');"
    docker exec $Container psql -U $DbUser -d $DbName -c $insertSql | Out-Null

    Write-Host ('[OK]   ' + $key)
    $applied++
}

Write-Host ''
Write-Host ('Migration complete: ' + $applied + ' applied, ' + $skipped + ' skipped.')
