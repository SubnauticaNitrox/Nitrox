using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    // The IncubatorActivationTerminal class is attached to the ion cube pillar near the incubator.  This is what the player clicks
    // power up the main incubator terminal window.
    public class IncubatorActivationTerminal_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IncubatorActivationTerminal t) => t.OnHandClick(default(GUIHand)));

        public static void Prefix(IncubatorActivationTerminal __instance)
        {
            if (!__instance.incubator.powered && Inventory.main.container.Contains(TechType.PrecursorIonCrystal))
            {
                // the server only knows about the main incubator platform which is the direct parent
                GameObject platform = __instance.gameObject.transform.parent.gameObject;
                NitroxId id = NitroxEntity.GetId(platform);
                IncubatorMetadata metadata = new(true, false);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, metadata);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
