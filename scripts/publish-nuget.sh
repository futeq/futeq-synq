#!/usr/bin/env bash
set -euo pipefail
shopt -s nullglob

echo "== publish.sh: pushing artifacts/*.nupkg to NuGet"

# Fail if missing key
: "${NUGET_API_KEY:?NUGET_API_KEY not set}"

packages=(artifacts/*.nupkg)

if [ ${#packages[@]} -eq 0 ]; then
  echo "No packages to publish"
  exit 1
fi

for f in "${packages[@]}"; do
  echo "Pushing \"$f\""
  dotnet nuget push "$f" \
    --api-key "$NUGET_API_KEY" \
    --source "https://api.nuget.org/v3/index.json" \
    --skip-duplicate
done