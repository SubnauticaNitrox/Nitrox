using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Services;
using NitroxClient.Services.Multiplayer;
using UnityEngine;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Changes the piece color during the placing process if the current base is desynced.
/// </summary>
public sealed partial class Builder_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Builder.Update());
    private static readonly Color DESYNCED_COLOR = Color.magenta;

    public static void Postfix()
    {
        if (!Builder.canPlace || !BuildingService.Main || !NitroxPrefs.SafeBuilding.Value)
        {
            return;
        }
        BaseGhost baseGhost = Builder.ghostModel.GetComponent<BaseGhost>();
        GameObject parentBase;
        if (baseGhost && baseGhost.targetBase)
        {
            parentBase = baseGhost.targetBase.gameObject;
        }
        // In case it's a simple Constructable
        else
        {
            parentBase = Builder.placementTarget;
        }

        if (parentBase && parentBase.TryGetNitroxId(out NitroxId parentId) &&
            BuildingService.Main.EnsureTracker(parentId).IsDesynced())
        {
            Builder.canPlace = false;
            Builder.ghostStructureMaterial.SetColor(ShaderPropertyID._Tint, DESYNCED_COLOR);
        }
    }
}
