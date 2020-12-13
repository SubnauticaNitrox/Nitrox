using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class PrecursorTeleporterActivationTerminal_OnProxyHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = typeof(PrecursorTeleporterActivationTerminal).GetMethod(nameof(PrecursorTeleporterActivationTerminal.OnProxyHandClick), BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(PrecursorTeleporterActivationTerminal __instance)
        {
            if (__instance.unlocked)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                PrecursorTeleporterActivationTerminalMetadata precursorTeleporterActivationTerminalMetadata = new PrecursorTeleporterActivationTerminalMetadata(__instance.unlocked);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, precursorTeleporterActivationTerminalMetadata);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
