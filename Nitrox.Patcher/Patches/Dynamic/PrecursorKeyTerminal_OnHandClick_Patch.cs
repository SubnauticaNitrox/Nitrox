using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
