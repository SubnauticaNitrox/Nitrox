using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class MaterialDrawer : IEditorDrawer<Material>
{
    public Material Draw(Material material)
    {
        // TODO: Implement Material picker
        GUILayout.Box(material.name, GUILayout.Width(150), GUILayout.Height(20));
        return material;
    }
}
