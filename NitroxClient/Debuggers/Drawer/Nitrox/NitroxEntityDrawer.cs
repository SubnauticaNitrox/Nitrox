using System.Linq;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Nitrox;

public class NitroxEntityDrawer : IDrawer<NitroxEntity>, IDrawer<NitroxId>
{
    private const float LABEL_WIDTH = 250;

    public void Draw(NitroxEntity nitroxEntity)
    {
        Draw(nitroxEntity.Id);

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("GameObject with IDs", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(NitroxEntity.GetGameObjects().Count().ToString());
        }

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Simulating state", GUILayout.Width(LABEL_WIDTH));
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
            GUILayout.Label("NitroxId", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(nitroxId == null ? "ID IS NULL!!!" : nitroxId.ToString());
        }

        GUILayout.Space(8);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Simulating state", GUILayout.Width(LABEL_WIDTH));
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
