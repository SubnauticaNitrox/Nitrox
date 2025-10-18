#!/bin/bash
# Publish Nitrox AppImage to GitHub Releases
# Usage: ./publish-appimage.sh <tag>

set -e

TAG="$1"
if [ -z "$TAG" ]; then
  echo "Usage: $0 <release-tag>"
  exit 1
fi

# Requires gh CLI: https://cli.github.com/
if ! command -v gh &> /dev/null; then
  echo "gh CLI not found. Install from https://cli.github.com/"
  exit 1
fi

APPIMAGE="NitroxLauncher-x86_64.AppImage"
if [ ! -f "$APPIMAGE" ]; then
  echo "$APPIMAGE not found. Build it first."
  exit 1
fi

gh release upload "$TAG" "$APPIMAGE" --repo <your-github-org>/<your-repo>

echo "AppImage uploaded to GitHub Release $TAG. To list on AppImageHub, submit your release URL at https://appimage.github.io/submit/"
