using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents non simulating player from applying upwards forces to PipeSurfaceFloater. Instead, detect when it reaches the surface and then
/// notify <see cref="RemotelyControlledPipeFloater"/>.
/// </summary>
public sealed partial class PipeSurfaceFloater_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((PipeSurfaceFloater t) => t.FixedUpdate());

    public static bool Prefix(PipeSurfaceFloater __instance)
    {
        bool isSimulated = __instance.TryGetIdOrWarn(out NitroxId pipeFloaterId) && Resolve<SimulationOwnership>().HasAnyLockType(pipeFloaterId);

        if (!isSimulated)
        {
            __instance.deployed = true;

            if (!__instance.rigidBody.isKinematic && __instance.transform.position.y >= -0.1f &&
                __instance.TryGetComponent(out RemotelyControlledPipeFloater remotelyControlledPipeFloater))
            {
                remotelyControlledPipeFloater.SetPositioned();
            }
        }

        return isSimulated;
    }
}
