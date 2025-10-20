using System.Reflection;
using NitroxClient.GameLogic.Bases;
using Nitrox.Model.DataStructures;

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

        BuildUtils.DeconstructionAllowed(parentId, ref isAllowed, ref message);
        if (!isAllowed)
        {
            Log.InGame(message);
            return false;
        }

        return true;
    }
}
