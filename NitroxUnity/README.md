# Nitrox Unity
A unity project to create prefabs which can be loaded into Subnautica at runtime.

## Setup
To setup Nitrox Unity and Subnautica you have to follow these steps:

1. If you start Unity the TunderKit window should appear. If not open it under `Tools->ThunderKit->Settings`. If the window is open click `Locate Game` and select `Subnautica.exe`. This can take 10-20 minutes.
2. Now click on `Tools->SubnauticaImporter->Asset Importer`. This can take multiple hours (2-5).
3. After it's finished loading click `Tools->SubnauticaImporter->All SN-Asset Fixes` This can take 30 minutes to 1 hour.

## Creating an Asset Bundle
To create an AssetBundle create an folder in `Assets/Nitrox/AssetBundles` and create a prefab in it with the same name (very important). In addition to that the prefab needs to be added to an AssetBundle (with the same name as the folder). For that select the prefab in the folder view and take the necessary steps in the lower part of the inspector window.

If you are creating an AssetBundle you can freely use Subnautica assets. Johnlok gave the permission to use them in a non commercial way (see [here](https://cdn.discordapp.com/attachments/528915769107415040/881663755010842624/Discord_fPrY9SeSun.jpg) and [here](https://cdn.discordapp.com/attachments/528915769107415040/881663762199883786/Discord_Pnd9ZVoLhw.jpg)).

You then can load the asset in the Nitrox codebase with `NitroxClient.Unity.Helper.AssetBundleLoader`.
