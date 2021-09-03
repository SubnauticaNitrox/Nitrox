#if SUBNAUTICA
using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    // The incubator class is attached to the main terminal window.  This is what the player clicks
    // to hatch the eggs using the enzymes.
    public class Incubator_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Incubator);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

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

                HandInteraction<Incubator> context = new HandInteraction<Incubator>(__instance, hand);
                LockRequest<HandInteraction<Incubator>> lockRequest = new LockRequest<HandInteraction<Incubator>>(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

                simulationOwnership.RequestSimulationLock(lockRequest);
            }

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<Incubator> context)
        {
            if (lockAquired)
            {
                IncubatorMetadata metadata = new IncubatorMetadata(true, true);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, metadata);

                skipPrefix = true;
                TARGET_METHOD.Invoke(context.Target, new[] { context.GuiHand });
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
#endif
