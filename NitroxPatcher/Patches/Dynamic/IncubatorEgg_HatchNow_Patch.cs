using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Handles the Sea Emperor baby hatching sequence in multiplayer.
/// All players see the full hatching animation for better visual consistency.
/// The simulating player spawns the real networked baby that will call SwimToMother().
/// Non-simulating players create temporary babies just for the animation sequence.
/// </summary>
public sealed partial class IncubatorEgg_HatchNow_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IncubatorEgg t) => t.HatchNow());

    public static bool Prefix(IncubatorEgg __instance)
    {
        // Play hatching visual/sound effects for all players
        __instance.fxControl.Play();
        Utils.PlayFMODAsset(__instance.hatchSound, __instance.transform, 30f);

        NitroxEntity serverKnownParent = __instance.GetComponentInParent<NitroxEntity>();
        if (!serverKnownParent)
        {
            Log.Error("Could not find a server known parent for incubator egg");
            return false;
        }

        bool isSimulating = Resolve<SimulationOwnership>().HasAnyLockType(serverKnownParent.Id);

        // Create baby GameObject for animation (networked for simulating player, temporary for others)
        GameObject baby = Object.Instantiate(__instance.seaEmperorBabyPrefab);
        baby.transform.SetParent(__instance.attachPoint);
        baby.transform.localPosition = Vector3.zero;
        baby.transform.localRotation = Quaternion.identity;

        if (isSimulating)
        {
            // Simulating player: make this baby networked and broadcast it
            NitroxId babyId = NitroxEntity.GenerateNewId(baby);
            WorldEntity entity = new(
                baby.transform.position.ToDto(),
                baby.transform.rotation.ToDto(),
                baby.transform.localScale.ToDto(),
                TechType.SeaEmperorBaby.ToDto(),
                3,
                "09883a6c-9e78-4bbf-9561-9fa6e49ce766",
                false,
                babyId,
                null);
            Resolve<Entities>().BroadcastEntitySpawnedByClient(entity);
        }
        else
        {
            // Non-simulating player: mark baby as temporary (will be cleaned up after animation)
            baby.name += "_NitroxTemporary";
        }

        // All players run the full animation sequence
        __instance.babyGO = baby;
        __instance.animationController.StartHatchAnimation(__instance.babyIdentifier, __instance.animParameter, baby);
        __instance.Invoke("PlayFxOnBaby", 2f);

        return false;
    }
}
