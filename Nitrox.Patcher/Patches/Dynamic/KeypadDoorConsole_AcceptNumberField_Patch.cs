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
