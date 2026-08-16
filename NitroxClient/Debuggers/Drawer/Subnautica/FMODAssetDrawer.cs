using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Subnautica;

internal sealed class FMODAssetDrawer : IDrawer<FMODAsset>
{
    public void Draw(FMODAsset asset)
    {
        GUILayout.TextField(asset ? asset.path : "NULL");
    }
}
