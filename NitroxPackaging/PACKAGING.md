# Nitrox Packaging Guide: Flatpak & AppImage

This guide explains how to build and package Nitrox as Flatpak and AppImage using the provided scripts and GitHub Actions workflow.

## Prerequisites
- .NET 9.0 SDK installed (locally or in CI)
- Flatpak and flatpak-builder installed (for Flatpak)
- Bash, wget (for AppImage)

## Packaging on Release
Packaging is automated via GitHub Actions and runs only when a release is published. Artifacts are uploaded for download.

### Files Added
- `com.subnauticanitrox.Nitrox.json`: Flatpak manifest
- `build-appimage.sh`: AppImage packaging script
- `.github/workflows/package.yml`: GitHub Actions workflow

## Manual Packaging

### Flatpak
1. Build the project:
   ```bash
   dotnet build Nitrox.Launcher/Nitrox.Launcher.csproj -c Release
   ```
2. Run Flatpak builder:
   ```bash
   flatpak-builder --force-clean --repo=repo build-dir com.subnauticanitrox.Nitrox.json
   ```

### AppImage
1. Build the project:
   ```bash
   dotnet build Nitrox.Launcher/Nitrox.Launcher.csproj -c Release
   ```
2. Run the packaging script:
   ```bash
   chmod +x build-appimage.sh
   ./build-appimage.sh
   ```

## Customization
- Update the manifest and script if your main binary or assets change location.
- Ensure your icon is present at `Nitrox.Launcher/Assets/nitrox.png`.

## For Maintainers
- To integrate, copy the above files into your fork or main repo.
- The workflow only triggers on release events.
- Artifacts are available in the GitHub Actions run after release.

---
For questions, contact the Nitrox packaging maintainers.
