#!/bin/bash
# Publish Nitrox Flatpak to Flathub
# Usage: ./publish-flathub.sh <your-flathub-fork-path>

set -e

FLATHUB_REPO_PATH="$1"
if [ -z "$FLATHUB_REPO_PATH" ]; then
  echo "Usage: $0 <path-to-your-flathub-fork>"
  exit 1
fi

cp com.subnauticanitrox.Nitrox.json "$FLATHUB_REPO_PATH/com.subnauticanitrox.Nitrox.json"
cd "$FLATHUB_REPO_PATH"
git add com.subnauticanitrox.Nitrox.json
git commit -m "Update Nitrox Flatpak manifest"
git push

echo "Now open a Pull Request to https://github.com/flathub/flathub"
