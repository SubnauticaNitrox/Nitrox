<p align="center">
    <img src="https://i.imgur.com/ofnNX5z.gif" alt="Nitrox Subnautica Multiplayer Mod" />
</p>

# Subnautica Nitrox
An open-source, multiplayer modification for the game <a href="https://unknownworlds.com/subnautica/">Subnautica</a>.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/SubnauticaNitrox/Nitrox)](https://github.com/SubnauticaNitrox/Nitrox/releases)
[![Translation status](https://hosted.weblate.org/widgets/subnauticanitrox/-/svg-badge.svg)](https://hosted.weblate.org/engage/subnauticanitrox/)
[![Discord](https://img.shields.io/discord/525437013403631617?logo=discord&logoColor=white)](https://discord.gg/E8B4X9s)

## MacOS Preview Version
This Nitrox 1.8.0.0 version is basically the [Linux launcher PR](https://github.com/SubnauticaNitrox/Nitrox/pull/1848) merged with several other PRs for testing.  
Merged PRs in this version:
1. [Linux launcher](https://github.com/SubnauticaNitrox/Nitrox/pull/1848)
2. [Crops syncing](https://github.com/SubnauticaNitrox/Nitrox/pull/2137)
3. [Waterpark (Alien Containment) syncing](https://github.com/SubnauticaNitrox/Nitrox/pull/2132)
4. [Teleport Vehicle fix](https://github.com/SubnauticaNitrox/Nitrox/pull/2111)
5. and finally [Leviathans sync](https://github.com/SubnauticaNitrox/Nitrox/pull/2106)

Additional files I added for macos:
1. `launchNitrox.command` simply runs the correct Nitrox.Launcher
2. `whitelistNitrox.command` disables Gatekeeper for the rest of the Nitrox folder (and only the Nitrox folder, to avoid needing to disable gatekeeper for the entire machine).
3. `steam_appid.txt` mitigates a bug where Subnautica thinks Steam isn't opened and crashes due to piracy protection. It is copied to the correct directory by the whitelist command.
4. `packageLauncher` makes a nice zip of the launcher and server files, and it isn't presented to the user

Hopefully in the future we can get a .app for Nitrox instead of the currently messy `.command` setup (and also avoid needing to install .NET)
