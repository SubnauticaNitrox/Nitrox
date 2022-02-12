using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Sealed_Weld_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Sealed t) => t.Weld(default(float)));

        public static void Postfix(Sealed __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            SealedDoorMetadata doorMetadata = new(__instance._sealed, __instance.openedAmount);
            Entities entities = NitroxServiceLocator.LocateService<Entities>();

            entities.BroadcastMetadataUpdate(id, doorMetadata);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
