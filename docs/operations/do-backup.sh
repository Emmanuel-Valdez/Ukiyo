#!/usr/bin/env bash
set -euo pipefail

STORE="${1:?Usage: do-backup.sh <vaultshop|ukiyostudio>}"

case "$STORE" in
    vaultshop)
        DB_KEY="POSTGRES_DB"
        BUCKET_KEY="VAULTSHOP_MINIO_BUCKET"
        MINIO_USER_KEY="VAULTSHOP_MINIO_USER"
        MINIO_PASS_KEY="VAULTSHOP_MINIO_PASSWORD"
        ;;
    ukiyostudio)
        DB_KEY="UKIYO_POSTGRES_DATABASE"
        BUCKET_KEY="UKIYO_MINIO_BUCKET"
        MINIO_USER_KEY="UKIYO_MINIO_USER"
        MINIO_PASS_KEY="UKIYO_MINIO_PASSWORD"
        ;;
    *)
        echo "FAIL: unknown store '$STORE' (use vaultshop or ukiyostudio)"
        exit 1
        ;;
esac

PLATFORM_ENV="/opt/vaultshop/.platform.env"
[ -f "$PLATFORM_ENV" ] || { echo "FAIL: $PLATFORM_ENV not found"; exit 1; }

BACKUP_DIR="$HOME/$STORE-backups"
DATE=$(date +%F_%H%M)
LOG="$BACKUP_DIR/backup.log"

# Read the real names from the private platform env instead of hardcoding.
get() { grep -m1 -E "^$1=" "$PLATFORM_ENV" | cut -d= -f2- || true; }

DB_NAME=$(get "$DB_KEY")
BUCKET_NAME=$(get "$BUCKET_KEY")
MINIO_USER=$(get "$MINIO_USER_KEY")
MINIO_PASSWORD=$(get "$MINIO_PASS_KEY")

if [ -z "$DB_NAME" ] || [ -z "$BUCKET_NAME" ] || [ -z "$MINIO_USER" ] || [ -z "$MINIO_PASSWORD" ]; then
    echo "FAIL: missing $DB_KEY/$BUCKET_KEY/$MINIO_USER_KEY/$MINIO_PASS_KEY in $PLATFORM_ENV"
    exit 1
fi

echo "[$(date '+%Y-%m-%d %H:%M')] === $STORE Backup ===" | tee -a "$LOG"

mkdir -p "$BACKUP_DIR/postgres" "$BACKUP_DIR/minio"
cd /opt/vaultshop || { echo "FAIL: /opt/vaultshop not found" | tee -a "$LOG"; exit 1; }

# --- PostgreSQL ---
echo "[$(date '+%H:%M')] PostgreSQL dump ($DB_NAME)..." | tee -a "$LOG"
PG_FILE="$BACKUP_DIR/postgres/${STORE}_$DATE.dump"
docker compose --env-file "$PLATFORM_ENV" -f docker-compose.platform.yml \
    exec -T -e DB_NAME="$DB_NAME" postgres \
    sh -c 'pg_dump -U "$POSTGRES_USER" -d "$DB_NAME" -Fc' > "$PG_FILE"

if [ ! -s "$PG_FILE" ]; then
    echo "FAIL: PostgreSQL dump is empty or missing" | tee -a "$LOG"
    exit 1
fi
echo "OK:   $(du -h "$PG_FILE" | cut -f1)" | tee -a "$LOG"

find "$BACKUP_DIR/postgres" -name "*.dump" -mtime +60 -delete

# --- MinIO (bucket propio, user scoped de la tienda) ---
echo "[$(date '+%H:%M')] MinIO bucket mirror ($BUCKET_NAME)..." | tee -a "$LOG"
MINIO_FILE="$BACKUP_DIR/minio/${BUCKET_NAME}_$DATE.tar.gz"
docker run --rm --network host \
    -v "$BACKUP_DIR/minio":/backup \
    -e MINIO_USER="$MINIO_USER" \
    -e MINIO_PASSWORD="$MINIO_PASSWORD" \
    -e BUCKET_NAME="$BUCKET_NAME" \
    --entrypoint sh \
    minio/mc:latest -ec '
        mc alias set local http://127.0.0.1:9000 "$MINIO_USER" "$MINIO_PASSWORD" >/dev/null
        mc mirror --overwrite "local/$BUCKET_NAME" "/backup/$BUCKET_NAME"
    '
tar czf "$MINIO_FILE" -C "$BACKUP_DIR/minio" "$BUCKET_NAME"
rm -rf "$BACKUP_DIR/minio/$BUCKET_NAME"

if [ ! -s "$MINIO_FILE" ]; then
    echo "FAIL: MinIO archive is empty or missing" | tee -a "$LOG"
    exit 1
fi
echo "OK:   $(du -h "$MINIO_FILE" | cut -f1)" | tee -a "$LOG"

find "$BACKUP_DIR/minio" -name "*.tar.gz" -mtime +60 -delete

# --- Resumen ---
echo "" | tee -a "$LOG"
echo "[$(date '+%Y-%m-%d %H:%M')] $STORE backup complete" | tee -a "$LOG"
ls -lh "$BACKUP_DIR/postgres/" "$BACKUP_DIR/minio/" | tee -a "$LOG"