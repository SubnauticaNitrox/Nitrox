using System.Linq;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Nitrox;

public class NitroxEntityDrawer : IDrawer<NitroxEntity>, IDrawer<NitroxId>
{
    public void Draw(NitroxEntity nitroxEntity)
    {
        DrawNitroxIdField(nitroxEntity.Id);

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("GameObject with IDs", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(NitroxEntity.GetGameObjects().Count().ToString());
        }

        GUILayout.Space(8);

        DrawSimulatingStateField(nitroxEntity.Id);
    }

    public void Draw(NitroxId nitroxId)
    {
        DrawNitroxIdField(nitroxId);

        GUILayout.Space(8);

        DrawSimulatingStateField(nitroxId);
    }

    private static void DrawNitroxIdField(NitroxId? nitroxId)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("NitroxId", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(nitroxId == null ? "<null>" : nitroxId.ToString());
        }
    }

    private static void DrawSimulatingStateField(NitroxId? nitroxId)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Simulating state", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (NitroxServiceLocator.Cache<SimulationOwnership>.Value.TryGetLockType(nitroxId, out SimulationLockType simulationLockType))
            {
                GUILayout.TextField(simulationLockType.ToString());
                return;
            }

            GUILayout.TextField("NONE");
        }
    }
}
