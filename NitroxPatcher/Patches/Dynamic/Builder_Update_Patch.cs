using NitroxModel.Helper;
using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;
using NitroxModel.DataStructures;
using UnityEngine;
using NitroxClient.Helpers;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Builder_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Builder.Update());

    public static void Postfix()
    {
        if (!Builder.canPlace || !BuildingHandler.Main)
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
            BuildingHandler.Main.EnsureTracker(parentId).IsDesynced() && NitroxPrefs.SafeBuilding.Value)
        {
            Builder.canPlace = false;
            Color safeColor = Color.magenta;
            IBuilderGhostModel[] components = Builder.ghostModel.GetComponents<IBuilderGhostModel>();
            for (int i = 0; i < components.Length; i++)
            {
                components[i].UpdateGhostModelColor(Builder.canPlace, ref safeColor);
            }
            Builder.ghostStructureMaterial.SetColor(ShaderPropertyID._Tint, safeColor);
        }
    }
}
