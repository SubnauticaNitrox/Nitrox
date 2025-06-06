#if SUBNAUTICA
using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Patch to disable the EscapePodFirstUseCinematicsController.OnSceneObjectsLoaded method when in multiplayer.
/// Initialize will be called OnSceneLoad to setup cinematics or first use cinematics
/// </summary>
public sealed partial class EscapePodFirstUseCinematicsController_OnSceneObjectsLoaded_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePodFirstUseCinematicsController t) => t.OnSceneObjectsLoaded());

    public static bool Prefix(EscapePodFirstUseCinematicsController __instance) => !Multiplayer.Active;
}
#endif
