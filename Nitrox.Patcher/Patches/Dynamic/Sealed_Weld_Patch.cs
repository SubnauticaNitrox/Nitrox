using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
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
