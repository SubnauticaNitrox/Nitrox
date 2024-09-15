using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Subnautica;

public class FMODAssetDrawer : IDrawer<FMODAsset>
{
    public void Draw(FMODAsset asset)
    {
        GUILayout.TextField(asset ? asset.path : "NULL");
    }
}
