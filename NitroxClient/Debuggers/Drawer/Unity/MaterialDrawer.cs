using System;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class MaterialDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Material) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Material material:
                Draw(material);
                break;
        }
    }

    public static Material Draw(Material material)
    {
        // TODO: Implement Material picker
        GUILayout.Box(material.name, GUILayout.Width(150), GUILayout.Height(20));
        return material;
    }
}
