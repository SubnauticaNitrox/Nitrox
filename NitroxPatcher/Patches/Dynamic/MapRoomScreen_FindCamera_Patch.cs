using System.Reflection;
using NitroxClient.GameLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomScreen_FindCamera_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomScreen t) => t.FindCamera(default));

    public static void Prefix()
    {
        MapRoomCameraIdentity.NormalizeCameraNumbers();
    }
}
