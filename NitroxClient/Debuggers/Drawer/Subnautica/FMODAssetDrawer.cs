using System;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Subnautica;

public class FMODAssetDrawer : IDrawer
{
    private const float LABEL_WIDTH = 150;

    public Type[] ApplicableTypes { get; } = { typeof(FMODAsset) };

    public void Draw(object target)
    {
        switch (target)
        {
            case FMODAsset asset:
                DrawFMODAsset(asset);
                break;
        }
    }

    private static void DrawFMODAsset(FMODAsset asset)
    {
        GUILayout.TextField(asset ? asset.path : "NULL");
    }
}
