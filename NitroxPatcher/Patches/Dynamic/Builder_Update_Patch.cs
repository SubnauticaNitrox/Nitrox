using HarmonyLib;
using NitroxModel.Helper;
using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

internal class Builder_Update_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method(() => Builder.Update());

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

        if (parentBase && NitroxEntity.TryGetIdFrom(parentBase, out NitroxId parentId) &&
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

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
