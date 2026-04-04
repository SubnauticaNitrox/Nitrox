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
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("NitroxId", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(nitroxEntity.Id == null ? "<null>" : nitroxEntity.Id.ToString());
        }

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("GameObject with IDs", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(NitroxEntity.GetGameObjects().Count().ToString());
        }

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Simulating state", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (NitroxServiceLocator.Cache<SimulationOwnership>.Value.TryGetLockType(nitroxEntity.Id, out SimulationLockType simulationLockType))
            {
                GUILayout.TextField(simulationLockType.ToString());
            }
            else
            {
                GUILayout.TextField("NONE");
            }
        }
    }

    public void Draw(NitroxId nitroxId)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("NitroxId", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(nitroxId == null ? "<null>" : nitroxId.ToString());
        }

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Simulating state", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (NitroxServiceLocator.Cache<SimulationOwnership>.Value.TryGetLockType(nitroxId, out SimulationLockType simulationLockType))
            {
                GUILayout.TextField(simulationLockType.ToString());
            }
            else
            {
                GUILayout.TextField("NONE");
            }
        }
    }
}
