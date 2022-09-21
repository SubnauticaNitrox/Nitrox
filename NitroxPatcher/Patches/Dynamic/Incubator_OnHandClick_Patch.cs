using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    // The incubator class is attached to the main terminal window.  This is what the player clicks
    // to hatch the eggs using the enzymes.
    public class Incubator_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Incubator t) => t.OnHandClick(default(GUIHand)));

        private static bool skipPrefix = false;

        public static bool Prefix(Incubator __instance, GUIHand hand)
        {
            if (skipPrefix)
            {
                return true;
            }

            // Request a simulation lock on the incubator so that we can authoritatively spawn the resulting creatures 
            if (__instance.powered && !__instance.hatched && Inventory.main.container.Contains(TechType.HatchingEnzymes))
            {
                SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

                // the server only knows about the the main incubator platform which is the direct parent
                GameObject platform = __instance.gameObject.transform.parent.gameObject;
                NitroxId id = NitroxEntity.GetId(platform);

                HandInteraction<Incubator> context = new(__instance, hand);
                LockRequest<HandInteraction<Incubator>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

                simulationOwnership.RequestSimulationLock(lockRequest);
            }

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<Incubator> context)
        {
            if (lockAquired)
            {
                IncubatorMetadata metadata = new(true, true);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, metadata);

                skipPrefix = true;
                context.Target.OnHandClick(context.GuiHand);
                skipPrefix = false;
            }
            else
            {
                context.Target.gameObject.AddComponent<DenyOwnershipHand>();
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
