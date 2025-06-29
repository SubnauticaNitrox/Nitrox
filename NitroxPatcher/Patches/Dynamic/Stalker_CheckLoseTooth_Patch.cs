using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Stalker_CheckLoseTooth_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Stalker t) => t.CheckLoseTooth(default(GameObject)));

    // HardnessMixin seems to be a bit buggy (ie: undefined values for some scraps, which is a vanilla bug), so we'll just hard-code the values for now.
    // Note that HardnessMixin is only used by Stalkers
    public static bool Prefix(Stalker __instance, GameObject target)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId)
            && !Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return false;
        }


        TechType techType = CraftData.GetTechType(target);

        float dropProbability = techType switch
        {
            TechType.ScrapMetal => 0.25f, // https://subnautica.fandom.com/wiki/Metal_Salvage_(Subnautica)
            TechType.MapRoomCamera => 0.25f,
            TechType.Titanium or TechType.Silver or TechType.Gold or TechType.Kyanite or TechType.Copper or TechType.Nickel => 0.15f,
            _ => 0f,
        };

        if (dropProbability == 0)
        {
            return false;
        }

        // Random.value returns a random float within[0.0..1.0] (range is inclusive)
        if (Random.value < dropProbability && Random.value < 0.5f)
        {
            // Code from Stalker.LoseTooth() to avoid having another patch

            GameObject toothGameObject = Object.Instantiate(__instance.toothPrefab);
            toothGameObject.transform.position = __instance.loseToothDropLocation.transform.position;
            toothGameObject.transform.rotation = __instance.loseToothDropLocation.transform.rotation;

            if (toothGameObject.activeSelf && __instance.isActiveAndEnabled)
            {
                foreach (Collider collider in toothGameObject.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(__instance.stalkerBodyCollider, collider);
                }
            }

            Utils.PlayFMODAsset(__instance.loseToothSound, toothGameObject.transform, 20f);
            LargeWorldEntity.Register(toothGameObject);

            Resolve<Items>().Dropped(toothGameObject);
        }

        return false;
    }
}
