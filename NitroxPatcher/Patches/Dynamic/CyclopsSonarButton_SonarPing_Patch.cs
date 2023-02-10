using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// The sonar will stay on until the player leaves the vehicle and automatically turns on when they enter again (if sonar was on at that time).
/// </summary>
public class CyclopsSonarButton_SonarPing_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.SonarPing());

    public static bool Prefix(CyclopsSonarButton __instance)
    {
        SubRoot subRoot = __instance.subRoot;
        if (Player.main.currentSub != subRoot)
        {
            return false;
        }
        if (LocalPlayerHasLock(subRoot) && !subRoot.powerRelay.ConsumeEnergy(subRoot.sonarPowerCost, out float _))
        {
            __instance.TurnOffSonar();
            return false;
        }
        SNCameraRoot.main.SonarPing();
        __instance.soundFX.Play();
        return false;
    }

    private static bool LocalPlayerHasLock(SubRoot subRoot)
    {
        return NitroxEntity.TryGetEntityFrom(subRoot.gameObject, out NitroxEntity entity) && Resolve<SimulationOwnership>().HasExclusiveLock(entity.Id);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
