using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SeaMoth_onLightsToggled_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaMoth t) => t.onLightsToggled(default(bool)));

    public static void Postfix(SeaMoth __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            SeamothMetadata metadata = Resolve<SeamothMetadataExtractor>().Extract(__instance);
            Resolve<Entities>().BroadcastMetadataUpdate(id, metadata);
        }
    }
}
