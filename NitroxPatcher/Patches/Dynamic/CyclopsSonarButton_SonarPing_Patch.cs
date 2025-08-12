using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// The sonar will stay on until the player leaves the vehicle and automatically turns on when they enter again (if sonar was on at that time).
/// </summary>
public sealed partial class CyclopsSonarButton_SonarPing_Patch : NitroxPatch, IDynamicPatch
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
        return subRoot.TryGetNitroxId(out NitroxId entityId) && Resolve<SimulationOwnership>().HasExclusiveLock(entityId);
    }
}
