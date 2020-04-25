using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Sealed_Weld_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Sealed).GetMethod("Weld", BindingFlags.Public | BindingFlags.Instance);
        
        public static bool Prefix(Sealed __instance, out bool __state)
        {
            __state = __instance._sealed;
            return true;
        }

        public static void Postfix(Sealed __instance, bool __state)
        {
            if (__state != __instance._sealed)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                SealedDoorMetadata doorMetadata = new SealedDoorMetadata(__instance._sealed);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, doorMetadata);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
