using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

internal class BuilderTool_Construct_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.Construct(default, default, default));

    public static bool Prefix(Constructable c)
    {
        if (!BuildingHandler.Main || !c.tr.parent || !NitroxEntity.TryGetIdFrom(c.tr.parent.gameObject, out NitroxId parentId))
        {
            return true;
        }
        bool isAllowed = true;
        string message = "";
        Constructable_DeconstructionAllowed_Patch.DeconstructionAllowed(parentId, ref isAllowed, ref message);
        if (!isAllowed)
        {
            ErrorMessage.AddMessage(message);
            return false;
        }

        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
