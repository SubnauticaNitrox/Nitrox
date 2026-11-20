using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public sealed class MeshFilterDrawer : IDrawer<MeshFilter>
{
    public void Draw(MeshFilter meshFilter)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Mesh", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            GUILayout.Label(meshFilter.sharedMesh ? meshFilter.sharedMesh.name : "None", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
        }

        if (meshFilter.sharedMesh)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Vertices", NitroxGUILayout.DrawerLabel);
                NitroxGUILayout.Separator();
                GUILayout.Label(meshFilter.sharedMesh.vertexCount.ToString(), GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Submeshes", NitroxGUILayout.DrawerLabel);
                NitroxGUILayout.Separator();
                GUILayout.Label(meshFilter.sharedMesh.subMeshCount.ToString(), GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }
    }
}
