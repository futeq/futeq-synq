#!/usr/bin/env bash
set -euo pipefail

# Inputs:
#   NEXT_VERSION (required) — provided by GitVersion (nuGetVersionV2) via env
# Behavior:
#   Packs ALL *.csproj under ./src into ./artifacts

echo "== pack-nuget.sh: packing src/** to artifacts with version: ${NEXT_VERSION:?NEXT_VERSION missing}"

mkdir -p artifacts

found=0
# Use -print0 to handle any spaces safely
while IFS= read -r -d '' csproj; do
  echo "Packing: $csproj"
  dotnet pack "$csproj" \
    -c Release \
    --no-build \
    -o artifacts \
    /p:ContinuousIntegrationBuild=true \
    /p:PackageVersion="$NEXT_VERSION"
  found=$((found+1))
done < <(find src -type f -name '*.csproj' -print0)

if [ "$found" -eq 0 ]; then
  echo "No .csproj found under ./src"
  exit 1
fi

echo "Packed $found project(s). Contents of artifacts/:"
ls -1 artifacts || true