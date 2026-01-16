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
/// The simulating player spawns the babies and broadcasts them to other players.
/// The simulating player also runs the full animation sequence which triggers SwimToMother().
/// Non-simulating players just see the egg animation - their babies are spawned via server
/// broadcast and will swim to mother via SeaEmperorBaby_Start_Patch.
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
        Validate.NotNull(serverKnownParent, "Could not find a server known parent for incubator egg");

        bool isSimulating = Resolve<SimulationOwnership>().HasAnyLockType(serverKnownParent.Id);

        if (isSimulating)
        {
            // Simulating player: spawn baby locally and run full animation sequence
            GameObject baby = Object.Instantiate(__instance.seaEmperorBabyPrefab);
            baby.transform.SetParent(__instance.attachPoint);
            baby.transform.localPosition = Vector3.zero;
            baby.transform.localRotation = Quaternion.identity;

            // Broadcast the baby entity to other players
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

            // Set up the baby reference and start the full animation sequence
            // The animation callback OnHatchAnimationEnd() will call baby.SwimToMother()
            __instance.babyGO = baby;
            __instance.animationController.StartHatchAnimation(__instance.babyIdentifier, __instance.animParameter, baby);
            __instance.Invoke("PlayFxOnBaby", 2f);
        }
        else
        {
            // Non-simulating player: just play the egg animation
            // The baby will be spawned by server broadcast and SeaEmperorBaby_Start_Patch
            // will make it swim to mother
            SafeAnimator.SetBool(__instance.animationController.eggAnimator, __instance.animParameter, true);
        }

        return false;
    }
}
