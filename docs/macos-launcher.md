# macOS launcher support

This document tracks the current macOS state of the Nitrox launcher. It is not a claim of full native macOS Nitrox client support.

## Prerequisites

- macOS 12 or newer.
- .NET 10 SDK.
- Git.
- Steam for native macOS Subnautica detection.
- Optional: Wine or a Wine-compatible wrapper when using a Windows Subnautica install.

## Build and run

From the repository root:

```bash
dotnet restore Nitrox.Launcher/Nitrox.Launcher.csproj
dotnet build Nitrox.Launcher/Nitrox.Launcher.csproj -c Debug
dotnet run --project Nitrox.Launcher/Nitrox.Launcher.csproj
```

The same commands are available through `scripts/macos-launcher.sh`:

```bash
scripts/macos-launcher.sh restore
scripts/macos-launcher.sh build
scripts/macos-launcher.sh run
```

To create a basic macOS app bundle zip:

```bash
dotnet build Nitrox.Launcher/Nitrox.Launcher.csproj -c Release -r osx-arm64 -p:CreateAppBundle=true
scripts/macos-launcher.sh bundle
```

Use `-r osx-x64` for Intel Macs. The bundle output is under `Nitrox.Launcher/bin/Release/net10.0/<rid>/Nitrox.app.zip`.

## Current behavior

- Launcher config/data uses the existing macOS `NitroxDirectory` mapping:
  - config, saves, logs, backups: `~/Library/Application Support/Nitrox`
  - cache: `~/Library/Caches/Nitrox`
- Native Steam detection checks the standard Steam data directory at `~/Library/Application Support/Steam`.
- Steam `libraryfolders.vdf` is parsed for additional library folders.
- Native macOS Steam installs can be resolved from the Steam common folder, `Subnautica.app`, or `Subnautica.app/Contents`.
- Manual path selection accepts:
  - `Subnautica.app`
  - `Subnautica.app/Contents`
  - the Steam common `Subnautica` folder that contains `Subnautica.app`
  - a Windows install root containing `Subnautica.exe` and `Subnautica_Data/Managed`
- Windows Subnautica installs inside common macOS Wine-style prefixes are searched when possible:
  - `~/.wine`
  - `~/.local/share/wineprefixes/*`
  - `~/Games/*` entries with `drive_c`
  - `~/Library/Application Support/CrossOver/Bottles/*`
- Standalone Windows executable launch on macOS uses `wine64` or `wine` from `PATH` and infers `WINEPREFIX` from paths containing `drive_c`.

## What does not work yet

### Launcher blockers

- The launcher still needs real-device UI verification on macOS with an installed .NET 10 SDK.
- The Wine path support is intentionally minimal. It does not manage Wine installation, bottle creation, or wrapper-specific launchers.
- Native Steam launch has not been verified end to end on macOS hardware in this change.

### Native macOS client/runtime blockers

- Native macOS multiplayer client injection is explicitly blocked in the launcher with a clear error.
- NitroxPatcher is still a `net472` patcher artifact and the runtime/injection path has not been ported or verified for native macOS Subnautica.
- No native macOS game-client compatibility claim should be made until the client injection/runtime is designed, built, and tested against the macOS game.

### Wine-wrapper blockers

- Wine launch requires a working `wine64` or `wine` executable on `PATH`.
- Steam overlay and Steam client integration inside Wine are not handled by the launcher yet.
- CrossOver-style paths are detected as prefixes, but CrossOver itself is not invoked or required by this project.

## Tests

Focused path and detection tests live in `Nitrox.Test/Model/Platforms/Discovery/GameInstallationHelperTest.cs`.

Run the relevant tests with:

```bash
dotnet test Nitrox.Test/Nitrox.Test.csproj --filter GameInstallationHelperTest
```

## Suggested next tasks

1. Verify the launcher UI on a macOS machine with .NET 10 SDK installed and enough disk space for restore/build.
2. Test native Steam detection with an actual `Subnautica.app` install.
3. Test Wine launch against a clean Wine prefix containing Steam and Windows Subnautica.
4. Decide whether Wine/Steam should get a first-class platform adapter instead of the current standalone Wine launch path.
5. Split native macOS client/runtime investigation into a separate project or issue; keep it outside launcher support work.
