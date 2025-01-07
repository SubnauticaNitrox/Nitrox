using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <remarks>
/// Prevents <see cref="CrashedShipExploder.Update"/> from occurring before initial sync has completed.
/// It lets us avoid a very weird edge case in which SetExplodeTime happens before server time is set on the client,
/// after what some event in this Update method might be triggered because there's a dead frame before the StoryGoalInitialSyncProcessor step
/// which sets up all the aurora story-related stuff locally.
/// </remarks>
public sealed partial class CrashedShipExploder_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashedShipExploder t) => t.Update());

    public static bool Prefix()
    {
        return Multiplayer.Main && Multiplayer.Main.InitialSyncCompleted;
    }
}
