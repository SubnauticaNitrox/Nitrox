using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Subnautica enables high precision physics any time a player is in a cyclops that is undergoing physics and is not actively anchored to the pilot seat. 
/// This is usually rare in the base game (mostly a player disconnecting from the pilot seat); however, it is the normal case for a passanger in nitrox. We
/// disable the switching as it causing oscillation of the interpolation.  Instead, any time someone is in the submarine we require high precision physics
/// if there is a remote player actively piloting.
/// </summary>
public class Player_RequiresHighPrecisionPhysics_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.RequiresHighPrecisionPhysics());

    public static bool Prefix(ref bool __result)
    {
        if (Player.main.currentSub)
        {
            MultiplayerCyclops cyclops = Player.main.currentSub.GetComponent<MultiplayerCyclops>();

            if (cyclops)
            {
                __result = (cyclops.CurrentPlayer != null);
                return false;
            }
        }

        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
