using System.Collections;
using System.Reflection;
using NitroxClient.GameLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomCamera_Start_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.Start());

    public static void Postfix(MapRoomCamera __instance)
    {
        __instance.StartCoroutine(RequestNumberAfterCameraStart(__instance));
    }

    private static IEnumerator RequestNumberAfterCameraStart(MapRoomCamera camera)
    {
        yield return null;

        if (!camera)
        {
            yield break;
        }

        if (camera.dockingPoint)
        {
            MapRoomCameraIdentity.NormalizeCameraNumbers();
            yield break;
        }

        MapRoomCameraIdentity.NormalizeCameraNumbers();

        yield return MapRoomCameraIdentity.RequestCameraNumberWhenReady(camera);

        MapRoomCameraIdentity.NormalizeCameraNumbers();
    }
}
