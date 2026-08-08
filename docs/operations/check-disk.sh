#!/usr/bin/env bash
set -euo pipefail

WARN_PCT="${1:-80}"
CRIT_PCT="${2:-90}"
EXIT_CODE=0

echo "=== Disk Usage Check ==="
echo "Warning:  ${WARN_PCT}%"
echo "Critical: ${CRIT_PCT}%"
echo ""

while IFS='' read -r line; do
    PCT=$(echo "$line" | awk '{print $2}' | tr -d '%')
    SOURCE=$(echo "$line" | awk '{print $1}')
    MOUNT=$(echo "$line" | awk '{print $3}')
    if [ "$PCT" -ge "$CRIT_PCT" ] 2>/dev/null; then
        echo "CRITICAL: $SOURCE ($MOUNT) at ${PCT}% (threshold: ${CRIT_PCT}%)"
        EXIT_CODE=2
    elif [ "$PCT" -ge "$WARN_PCT" ] 2>/dev/null; then
        echo "WARNING:  $SOURCE ($MOUNT) at ${PCT}% (threshold: ${WARN_PCT}%)"
        [ "$EXIT_CODE" -lt 1 ] && EXIT_CODE=1
    else
        echo "OK:       $SOURCE ($MOUNT) at ${PCT}%"
    fi
done < <(df -h --output=source,pcent,target | tail -n +2)

echo ""
echo "Exit code: $EXIT_CODE"
exit $EXIT_CODE
