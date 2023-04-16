using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_SignInput_OnDeselect_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.OnDeselect());

    public static void Postfix(uGUI_SignInput __instance)
    {
        PrefabIdentifier parentIdentifier = __instance.gameObject.FindAncestor<PrefabIdentifier>();
        if (parentIdentifier && NitroxEntity.TryGetIdOrWarn(parentIdentifier.gameObject, out NitroxId id))
        {
            EntitySignMetadata metadata = new(__instance.text, __instance.colorIndex, __instance.scaleIndex, __instance.elementsState, __instance.IsBackground());
            Resolve<Entities>().BroadcastMetadataUpdate(id, metadata);
        }
    }
}
