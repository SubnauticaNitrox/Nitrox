#!/bin/bash
set -e

APPDIR=AppDir
mkdir -p "$APPDIR/usr/bin"
cp -r Nitrox.Launcher/bin/Release/net9.0/linux-x64/publish/* "$APPDIR/usr/bin/"

# Create desktop entry
mkdir -p "$APPDIR/usr/share/applications"
echo "[Desktop Entry]\nName=Nitrox Launcher\nExec=NitroxLauncher\nIcon=nitrox\nType=Application\nCategories=Game;" > "$APPDIR/usr/share/applications/nitrox.desktop"

# Create icon
mkdir -p "$APPDIR/usr/share/icons/hicolor/256x256/apps"
cp Nitrox.Launcher/Assets/Images/subnautica-icon.png "$APPDIR/usr/share/icons/hicolor/256x256/apps/nitrox.png"

# Download appimagetool if not present
if [ ! -f appimagetool ]; then
  wget https://github.com/AppImage/AppImageKit/releases/latest/download/appimagetool-x86_64.AppImage -O appimagetool
  chmod +x appimagetool
fi

# Build AppImage
./appimagetool "$APPDIR" "NitroxLauncher-${ARCH}.AppImage"
