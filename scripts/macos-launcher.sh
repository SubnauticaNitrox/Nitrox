#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/Nitrox.Launcher/Nitrox.Launcher.csproj"
TEST_PROJECT="$ROOT_DIR/Nitrox.Test/Nitrox.Test.csproj"
RID="${NITROX_MACOS_RID:-osx-arm64}"

command="${1:-build}"

case "$command" in
  restore)
    dotnet restore "$PROJECT"
    ;;
  build)
    dotnet build "$PROJECT" -c Debug
    ;;
  run)
    dotnet run --project "$PROJECT"
    ;;
  test)
    dotnet test "$TEST_PROJECT" --filter GameInstallationHelperTest
    ;;
  bundle)
    dotnet build "$PROJECT" -c Release -r "$RID" -p:CreateAppBundle=true
    ;;
  *)
    echo "Usage: $0 [restore|build|run|test|bundle]" >&2
    exit 2
    ;;
esac
