using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.Services.Multiplayer;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class BuilderTool_Construct_Patch : NitroxPatch, IDynamicPatch
{
    private static BuildingService buildingService;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BuilderTool t) => t.Construct(default, default, default));

    public BuilderTool_Construct_Patch(BuildingService bs)
    {
        buildingService = bs ?? throw new ArgumentNullException(nameof(bs));
    }

    public static bool Prefix(Constructable c)
    {
        if (!c.tr.parent || !c.tr.parent.TryGetNitroxId(out NitroxId parentId))
        {
            return true;
        }

        bool isAllowed = true;
        string message = string.Empty;

        buildingService.DeconstructionAllowed(parentId, ref isAllowed, ref message);
        if (!isAllowed)
        {
            Log.InGame(message);
            return false;
        }

        return true;
    }
}
