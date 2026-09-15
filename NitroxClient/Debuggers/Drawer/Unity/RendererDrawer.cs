using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public sealed class RendererDrawer : IDrawer<MeshRenderer>, IDrawer<ParticleSystemRenderer>
{
    public void Draw(MeshRenderer renderer) => DrawRenderer(renderer);

    public void Draw(ParticleSystemRenderer renderer)
    {
        DrawRenderer(renderer);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Render Mode", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            renderer.renderMode = NitroxGUILayout.EnumPopup(renderer.renderMode, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Trail Material", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            GUILayout.Label(renderer.trailMaterial ? renderer.trailMaterial.name : "None", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
        }
    }

    private static void DrawRenderer(Renderer renderer)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Enabled", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            renderer.enabled = NitroxGUILayout.BoolField(renderer.enabled, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Shadow Casting", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            renderer.shadowCastingMode = NitroxGUILayout.EnumPopup(renderer.shadowCastingMode, NitroxGUILayout.VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Receive Shadows", NitroxGUILayout.DrawerLabel);
            NitroxGUILayout.Separator();
            renderer.receiveShadows = NitroxGUILayout.BoolField(renderer.receiveShadows, NitroxGUILayout.VALUE_WIDTH);
        }

        Material[] materials = renderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Material {i}", NitroxGUILayout.DrawerLabel);
                NitroxGUILayout.Separator();
                GUILayout.Label(materials[i] ? materials[i].name : "None", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }
    }
}
