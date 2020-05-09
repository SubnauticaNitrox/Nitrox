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
    public class KeypadDoorConsole_AcceptNumberField_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(KeypadDoorConsole);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("AcceptNumberField", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static void Postfix(KeypadDoorConsole __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            KeypadMetadata keypadMetadata = new KeypadMetadata(__instance.unlocked);
            
            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, keypadMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
