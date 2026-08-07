#!/usr/bin/env bash
set -euo pipefail

STORE="${1:?Usage: check-backup-freshness.sh <vaultshop|ukiyostudio> [max-age-hours]}"
MAX_AGE_HOURS="${2:-48}"
THRESHOLD_SECONDS=$((MAX_AGE_HOURS * 3600))
BACKUP_DIR="$HOME/$STORE-backups"
EXIT_CODE=0

echo "=== Backup Freshness Check ($STORE) ==="
echo "Max age: ${MAX_AGE_HOURS}h"
echo ""

# --- PostgreSQL ---
PG_DIR="$BACKUP_DIR/postgres"
if [ ! -d "$PG_DIR" ]; then
    echo "FAIL: PostgreSQL backup dir '$PG_DIR' not found"
    EXIT_CODE=1
else
    LATEST_PG=$(ls -t "$PG_DIR"/*.dump 2>/dev/null | head -1)
    if [ -z "$LATEST_PG" ]; then
        echo "FAIL: No PostgreSQL dump found in $PG_DIR"
        EXIT_CODE=1
    else
        AGE=$(($(date +%s) - $(stat -c %Y "$LATEST_PG")))
        AGE_HOURS=$((AGE / 3600))
        if [ "$AGE" -le "$THRESHOLD_SECONDS" ]; then
            echo "OK:   PostgreSQL dump is ${AGE_HOURS}h old ($(basename "$LATEST_PG"))"
        else
            echo "FAIL: PostgreSQL dump is ${AGE_HOURS}h old (max ${MAX_AGE_HOURS}h) ($(basename "$LATEST_PG"))"
            EXIT_CODE=1
        fi
    fi
fi

# --- MinIO ---
MINIO_DIR="$BACKUP_DIR/minio"
if [ ! -d "$MINIO_DIR" ]; then
    echo "FAIL: MinIO backup dir '$MINIO_DIR' not found"
    EXIT_CODE=1
else
    LATEST_MINIO=$(ls -t "$MINIO_DIR"/*.tar.gz 2>/dev/null | head -1)
    if [ -z "$LATEST_MINIO" ]; then
        echo "FAIL: No MinIO archive found in $MINIO_DIR"
        EXIT_CODE=1
    else
        AGE=$(($(date +%s) - $(stat -c %Y "$LATEST_MINIO")))
        AGE_HOURS=$((AGE / 3600))
        if [ "$AGE" -le "$THRESHOLD_SECONDS" ]; then
            echo "OK:   MinIO archive is ${AGE_HOURS}h old ($(basename "$LATEST_MINIO"))"
        else
            echo "FAIL: MinIO archive is ${AGE_HOURS}h old (max ${MAX_AGE_HOURS}h) ($(basename "$LATEST_MINIO"))"
            EXIT_CODE=1
        fi
    fi
fi

echo ""
echo "Exit code: $EXIT_CODE"
exit $EXIT_CODE