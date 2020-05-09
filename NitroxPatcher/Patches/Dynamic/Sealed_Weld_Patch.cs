using System;
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
        public static readonly Type TARGET_CLASS = typeof(Sealed);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Weld", BindingFlags.Public | BindingFlags.Instance);
        
        public static void Postfix(Sealed __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            SealedDoorMetadata doorMetadata = new SealedDoorMetadata(__instance._sealed, __instance.openedAmount);
            Entities entities = NitroxServiceLocator.LocateService<Entities>();

            entities.BroadcastMetadataUpdate(id, doorMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
