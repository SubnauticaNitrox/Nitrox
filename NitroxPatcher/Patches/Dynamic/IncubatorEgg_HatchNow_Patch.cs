#if SUBNAUTICA
using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IncubatorEgg_HatchNow_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IncubatorEgg);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HatchNow", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(IncubatorEgg __instance)
        {
            StartHatchingVisuals(__instance);

            SpawnBabiesIfSimulating(__instance);

            return false;
        }

        private static void StartHatchingVisuals(IncubatorEgg egg)
        {
            egg.fxControl.Play();
            Utils.PlayFMODAsset(egg.hatchSound, egg.transform, 30f);

            string animParameter = (string)egg.ReflectionGet("animParameter");
            SafeAnimator.SetBool(egg.animationController.eggAnimator, animParameter, true);
        }

        private static void SpawnBabiesIfSimulating(IncubatorEgg egg)
        {
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            NitroxEntity serverKnownParent = egg.GetComponentInParent<NitroxEntity>();
            Validate.NotNull(serverKnownParent, "Could not find a server known parent for incubator egg");

            // Only spawn the babies if we are simulating the main incubator platform. 
            if (simulationOwnership.HasAnyLockType(serverKnownParent.Id))
            {
                GameObject baby = UnityEngine.Object.Instantiate<GameObject>(egg.seaEmperorBabyPrefab);
                baby.transform.position = egg.attachPoint.transform.position;
                baby.transform.localRotation = Quaternion.identity;

                NitroxId babyId = NitroxEntity.GetId(baby);

                Entity entity = new Entity(baby.transform.position.ToDto(), baby.transform.rotation.ToDto(), baby.transform.localScale.ToDto(), TechType.SeaEmperorBaby.ToDto(), 3, "09883a6c-9e78-4bbf-9561-9fa6e49ce766", true, babyId, null);
                NitroxServiceLocator.LocateService<Entities>().BroadcastEntitySpawnedByClient(entity);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
#endif
