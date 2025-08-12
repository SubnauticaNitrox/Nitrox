#if SUBNAUTICA
using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <remarks>
/// Prevents <see cref="AuroraWarnings.Update"/> from occurring before initial sync has completed.
/// It lets us avoid a very weird edge case in which warnings are triggered way too early. Linked to <see cref="CrashedShipExploder_Update_Patch"/>
/// </remarks>
public sealed partial class AuroraWarnings_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((AuroraWarnings t) => t.Update());

    public static bool Prefix()
    {
        return Multiplayer.Main && Multiplayer.Main.InitialSyncCompleted;
    }
}
#endif
