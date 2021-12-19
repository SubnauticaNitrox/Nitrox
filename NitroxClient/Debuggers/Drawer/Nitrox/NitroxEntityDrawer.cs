using System;
using System.Linq;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Nitrox;

public class NitroxEntityDrawer : IDrawer
{
    private const float LABEL_WIDTH = 250;

    public Type[] ApplicableTypes { get; } = { typeof(NitroxEntity), typeof(NitroxId) };

    public void Draw(object target)
    {
        switch (target)
        {
            case NitroxEntity nitroxEntity:
                DrawNitroxEntity(nitroxEntity);
                break;
            case NitroxId nitroxId:
                DrawNitroxId(nitroxId);
                break;
        }
    }

    private static void DrawNitroxEntity(NitroxEntity nitroxEntity)
    {
        DrawNitroxId(nitroxEntity.Id);

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("GameObject with IDs", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(NitroxEntity.GetGameObjects().Count().ToString());
        }
    }

    private static void DrawNitroxId(NitroxId nitroxId)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("NitroxId", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(nitroxId == null ? "ID IS NULL!!!" : nitroxId.ToString());
        }
    }
}
