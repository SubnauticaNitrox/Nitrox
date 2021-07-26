using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class PrecursorKeyTerminal_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = typeof(PrecursorKeyTerminal).GetMethod(nameof(PrecursorKeyTerminal.OnHandClick), BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(PrecursorKeyTerminal __instance)
        {
            if (__instance.slotted)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                PrecursorKeyTerminalMetadata precursorKeyTerminalMetadata = new PrecursorKeyTerminalMetadata(__instance.slotted);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, precursorKeyTerminalMetadata);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
