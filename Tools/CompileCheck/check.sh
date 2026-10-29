#!/usr/bin/env bash
# Compile-check Assets/FortIdle/Scripts outside Unity. Needs the .NET 8 SDK.
# Catches syntax, type and API-shape errors. It does not replace Play Mode QA.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
refs="$here/.refs"
if [ ! -f "$refs/lib/netstandard2.0/UnityEngine.CoreModule.dll" ]; then
  mkdir -p "$refs"
  curl -sSL -o "$refs/modules.nupkg" \
    https://api.nuget.org/v3-flatcontainer/unityengine.modules/2021.3.33/unityengine.modules.2021.3.33.nupkg
  (cd "$refs" && unzip -qo modules.nupkg)
fi
log="$(dotnet build "$here/CompileCheck.csproj" -nologo -v q -clp:NoSummary -o "$here/.out" "$@" 2>&1 || true)"
errors="$(printf '%s\n' "$log" | grep -E ': error ' | sed -E 's/ \[[^]]*\]$//' | sort -u | grep -v -F -f <(grep -v '^#' "$here/unity6-only.txt" | sed '/^$/d') || true)"
if [ -n "$errors" ]; then
  printf '%s\n' "$errors" | sed "s|$(cd "$here/../.." && pwd)/||"
  echo "COMPILE CHECK FAILED: $(printf '%s\n' "$errors" | wc -l) error(s)"
  exit 1
fi
echo "COMPILE CHECK PASSED"
