using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Rocket_CallElevator_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Rocket t) => t.CallElevator(default(bool)));

    public static void Prefix(Rocket __instance, out Rocket.RocketElevatorStates __state)
    {
        __state = __instance.elevatorState;
    }

    public static void Postfix(Rocket __instance, bool up, Rocket.RocketElevatorStates __state)
    {
        if (__state != __instance.elevatorState && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
        }
    }
}
