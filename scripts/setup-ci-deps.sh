#!/bin/bash
set -e

# Define paths
# Default Steam path on macOS
STEAM_COMMON_PATH="$HOME/Library/Application Support/Steam/steamapps/common"
SUBNAUTICA_APP="$STEAM_COMMON_PATH/Subnautica/Subnautica.app"
GAME_MANAGED_DIR="$SUBNAUTICA_APP/Contents/Resources/Data/Managed"
GAME_STREAMING_ASSETS_DIR="$SUBNAUTICA_APP/Contents/Resources/Data/StreamingAssets"

# Output directory in the repo
REPO_DEPS_DIR="$(pwd)/deps"
OUTPUT_MANAGED_DIR="$REPO_DEPS_DIR/Resources/Data/Managed"
OUTPUT_VERSION_DIR="$REPO_DEPS_DIR/Resources/Data/StreamingAssets/SNUnmanagedData"

echo "Checking for Subnautica installation at: $SUBNAUTICA_APP"

if [ ! -d "$SUBNAUTICA_APP" ]; then
    echo "Error: Subnautica.app not found at default location."
    echo "Please ensure Subnautica is installed via Steam."
    exit 1
fi

echo "Found Subnautica. Creating dependency structure in 'deps/'..."

# Create directories
mkdir -p "$OUTPUT_MANAGED_DIR"
mkdir -p "$OUTPUT_VERSION_DIR"

# List of required DLLs based on Directory.Build.targets
REQUIRED_DLLS=(
    "Assembly-CSharp.dll"
    "Assembly-CSharp-firstpass.dll"
    "FMODUnity.dll"
    "Newtonsoft.Json.dll"
    "Unity.Addressables.dll"
    "Unity.ResourceManager.dll"
    "Unity.TextMeshPro.dll"
    "Unity.Timeline.dll"
    "UnityEngine.dll"
    "UnityEngine.AccessibilityModule.dll"
    "UnityEngine.AIModule.dll"
    "UnityEngine.AndroidJNIModule.dll"
    "UnityEngine.AnimationModule.dll"
    "UnityEngine.ARModule.dll"
    "UnityEngine.AssetBundleModule.dll"
    "UnityEngine.AudioModule.dll"
    "UnityEngine.ClothModule.dll"
    "UnityEngine.ClusterInputModule.dll"
    "UnityEngine.ClusterRendererModule.dll"
    "UnityEngine.CoreModule.dll"
    "UnityEngine.CrashReportingModule.dll"
    "UnityEngine.DirectorModule.dll"
    "UnityEngine.DSPGraphModule.dll"
    "UnityEngine.GameCenterModule.dll"
    "UnityEngine.GridModule.dll"
    "UnityEngine.HotReloadModule.dll"
    "UnityEngine.ImageConversionModule.dll"
    "UnityEngine.IMGUIModule.dll"
    "UnityEngine.InputLegacyModule.dll"
    "UnityEngine.InputModule.dll"
    "Unity.InputSystem.dll"
    "UnityEngine.JSONSerializeModule.dll"
    "UnityEngine.LocalizationModule.dll"
    "UnityEngine.ParticleSystemModule.dll"
    "UnityEngine.PerformanceReportingModule.dll"
    "UnityEngine.Physics2DModule.dll"
    "UnityEngine.PhysicsModule.dll"
    "UnityEngine.ProfilerModule.dll"
    "UnityEngine.ScreenCaptureModule.dll"
    "UnityEngine.SharedInternalsModule.dll"
    "UnityEngine.SpriteMaskModule.dll"
    "UnityEngine.SpriteShapeModule.dll"
    "UnityEngine.StreamingModule.dll"
    "UnityEngine.SubstanceModule.dll"
    "UnityEngine.TerrainModule.dll"
    "UnityEngine.TerrainPhysicsModule.dll"
    "UnityEngine.TextCoreModule.dll"
    "UnityEngine.TextRenderingModule.dll"
    "UnityEngine.TilemapModule.dll"
    "UnityEngine.TLSModule.dll"
    "UnityEngine.UI.dll"
    "UnityEngine.UIElementsModule.dll"
    "UnityEngine.UIModule.dll"
    "UnityEngine.UmbraModule.dll"
    "UnityEngine.UNETModule.dll"
    "UnityEngine.UnityAnalyticsModule.dll"
    "UnityEngine.UnityConnectModule.dll"
    "UnityEngine.UnityTestProtocolModule.dll"
    "UnityEngine.UnityWebRequestAssetBundleModule.dll"
    "UnityEngine.UnityWebRequestAudioModule.dll"
    "UnityEngine.UnityWebRequestModule.dll"
    "UnityEngine.UnityWebRequestTextureModule.dll"
    "UnityEngine.UnityWebRequestWWWModule.dll"
    "UnityEngine.VehiclesModule.dll"
    "UnityEngine.VFXModule.dll"
    "UnityEngine.VideoModule.dll"
    "UnityEngine.VRModule.dll"
    "UnityEngine.WindModule.dll"
    "UnityEngine.XRModule.dll"
    "Sentry.dll"
    "LitJson.dll"
)

echo "Copying DLLs..."
for dll in "${REQUIRED_DLLS[@]}"; do
    if [ -f "$GAME_MANAGED_DIR/$dll" ]; then
        cp "$GAME_MANAGED_DIR/$dll" "$OUTPUT_MANAGED_DIR/"
    else
        echo "Warning: $dll not found in game directory."
    fi
done

# Copy version file
VERSION_FILE="plastic_status.ignore"
if [ -f "$GAME_STREAMING_ASSETS_DIR/SNUnmanagedData/$VERSION_FILE" ]; then
    echo "Copying version file..."
    cp "$GAME_STREAMING_ASSETS_DIR/SNUnmanagedData/$VERSION_FILE" "$OUTPUT_VERSION_DIR/"
else
    echo "Warning: Version file not found. Creating a dummy one."
    echo "85000" > "$OUTPUT_VERSION_DIR/$VERSION_FILE"
fi

echo "Done! Dependencies copied to 'deps/'."
echo "Please commit the 'deps' folder to your repository to make it available for CI."
