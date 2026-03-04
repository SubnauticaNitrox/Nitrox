using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public sealed class MaterialDrawer : IEditorDrawer<Material>, IEditorDrawer<PhysicMaterial>
{
    public Material Draw(Material material)
    {
        // TODO: Implement Material picker
        GUILayout.Box(material.name, GUILayout.Width(150), GUILayout.Height(20));
        return material;
    }

    public PhysicMaterial Draw(PhysicMaterial target)
    {
        // TODO: Implement Material picker
        GUILayout.Box(target.name, GUILayout.Width(150), GUILayout.Height(20));
        return target;
    }
}
