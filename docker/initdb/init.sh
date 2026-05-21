#!/bin/bash
# Runs all DB migrations in order on first postgres init.
# Uses schema_migrations for tracking — consistent with migrate.ps1.
# Runs automatically from /docker-entrypoint-initdb.d/ on a fresh volume.
set -e

psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "
CREATE TABLE IF NOT EXISTS schema_migrations (
    script_name VARCHAR(255) PRIMARY KEY,
    applied_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);"

apply() {
    local key="$1"
    local path="$2"
    local exists
    exists=$(psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -tAc \
        "SELECT 1 FROM schema_migrations WHERE script_name='$key';")
    if [ "$exists" = "1" ]; then
        echo "[SKIP] $key"
    else
        echo "[OK]   $key"
        psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f "$path"
        psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c \
            "INSERT INTO schema_migrations (script_name) VALUES ('$key');"
    fi
}

for f in $(ls /docker-db-scripts/infrastructure/*.sql 2>/dev/null | sort); do
    apply "infrastructure/$(basename "$f")" "$f"
done

for f in $(ls /docker-db-scripts/data/*.sql 2>/dev/null | sort); do
    apply "data/$(basename "$f")" "$f"
done

echo "Migrations complete."
