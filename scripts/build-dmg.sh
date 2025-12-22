#!/bin/bash
set -e

# Configuration
APP_NAME="Nitrox"
PROJECT_PATH="Nitrox.Launcher/Nitrox.Launcher.csproj"
OUTPUT_DMG="Nitrox-Installer.dmg"
VOL_NAME="Nitrox Installer"
DMG_ROOT="dmg_root"

# Clean previous build
echo "Cleaning previous build..."
rm -rf "$DMG_ROOT" "$OUTPUT_DMG"
dotnet clean "$PROJECT_PATH"

# Build the application
echo "Building $APP_NAME..."
# Try to find dependencies in 'deps' folder if not set
if [ -z "$SUBNAUTICA_INSTALLATION_PATH" ] && [ -d "$(pwd)/deps" ]; then
    export SUBNAUTICA_INSTALLATION_PATH="$(pwd)/deps"
fi
# Fallback to /tmp/Subnautica if still not set (mostly for CI mock)
export SUBNAUTICA_INSTALLATION_PATH=${SUBNAUTICA_INSTALLATION_PATH:-"/tmp/Subnautica"}

dotnet build "$PROJECT_PATH" -r osx-arm64 -p:CreateAppBundle=true -p:Configuration=Release

# Prepare DMG root
echo "Preparing DMG root..."
mkdir -p "$DMG_ROOT"
cp -R "Nitrox.Launcher/bin/Release/net9.0/osx-arm64/bundle/$APP_NAME.app" "$DMG_ROOT/"

# Fix runtimeconfig.json for .NET 10 compatibility
CONFIG_PATH="$DMG_ROOT/$APP_NAME.app/Contents/MacOS/Nitrox.Launcher.runtimeconfig.json"
if [ -f "$CONFIG_PATH" ]; then
    echo "Patching runtimeconfig.json..."
    # Use sed to insert rollForward property
    sed -i '' 's/"tfm": "net9.0",/"rollForward": "Major",\n    "tfm": "net9.0",/' "$CONFIG_PATH"
fi

# Create link to Applications
ln -s /Applications "$DMG_ROOT/Applications"

# Create temporary DMG
echo "Creating temporary DMG..."
TMP_DMG="tmp.dmg"
rm -f "$TMP_DMG"
hdiutil create -srcfolder "$DMG_ROOT" -volname "$VOL_NAME" -fs HFS+ -fsargs "-c c=64,a=16,e=16" -format UDRW "$TMP_DMG"

# Mount the DMG
echo "Mounting DMG..."
DEVICE=$(hdiutil attach -readwrite -noverify -noautoopen "$TMP_DMG" | egrep '^/dev/' | sed 1q | awk '{print $1}')

# Wait a bit for the volume to be mounted
sleep 2

# Customize with AppleScript
echo "Customizing DMG appearance..."
osascript <<EOF
tell application "Finder"
    tell disk "$VOL_NAME"
        open
        set current view of container window to icon view
        set toolbar visible of container window to false
        set statusbar visible of container window to false
        set the bounds of container window to {400, 100, 900, 400}
        set theViewOptions to the icon view options of container window
        set arrangement of theViewOptions to not arranged
        set icon size of theViewOptions to 100
        set position of item "$APP_NAME.app" of container window to {140, 150}
        set position of item "Applications" of container window to {360, 150}
        close
        open
        update without registering applications
        delay 2
    end tell
end tell
EOF

# Unmount
echo "Unmounting DMG..."
hdiutil detach "$DEVICE"

# Convert to compressed DMG
echo "Compressing DMG..."
hdiutil convert "$TMP_DMG" -format UDZO -imagekey zlib-level=9 -o "$OUTPUT_DMG"

# Cleanup
rm -f "$TMP_DMG"
rm -rf "$DMG_ROOT"

echo "Done! Created $OUTPUT_DMG"
