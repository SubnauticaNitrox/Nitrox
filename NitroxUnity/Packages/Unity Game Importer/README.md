# Game Project Importer for Subnautica working with ThunderKit

This tool utilizes a modified version of [uTinyRipper](https://github.com/mafaca/UtinyRipper) to import game assets that continue to target game assemblies.

This allows you to easily setup a project for learning about the tools you're working with when making mods for games with modding support.

to install this package add the following to your ProjectRoot/Packages/manifest.json
``` "de.jannify.unitygameimporter":"https://github.com/Jannify/GameImporter-Subnautica.git", ```

This tool requires ThunderKit is installed in the project, go to https://github.com/PassivePicasso/ThunderKit to get started with ThunderKit in Unity.

Configure what you want to export from the game from the ThunderKit Settings window.

Make sure you Locate the Game using ThunderKit and have the games assemblies in your Packages folder before importing assets or the import will fail

Use the Main Menu/Tools/Game Asset Importer to begin importing assets from the configured game.
