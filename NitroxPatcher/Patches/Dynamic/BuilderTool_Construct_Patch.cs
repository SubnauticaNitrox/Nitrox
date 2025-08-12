using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxClient.Helpers;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BuilderTool_Construct_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.Construct(default, default, default));

    public static bool Prefix(Constructable c)
    {
        if (!BuildingHandler.Main || !c.tr.parent || !c.tr.parent.TryGetNitroxId(out NitroxId parentId))
        {
            return true;
        }

        bool isAllowed = true;
        string message = string.Empty;

        Constructable_DeconstructionAllowed_Patch.DeconstructionAllowed(parentId, ref isAllowed, ref message);
        if (!isAllowed)
        {
            Log.InGame(message);
            return false;
        }

        return true;
    }
}
