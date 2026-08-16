using System.Reflection;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomFunctionality_StartScanning_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomFunctionality t) => t.StartScanning(default));

    public static void Postfix(MapRoomFunctionality __instance, TechType newTypeToScan)
    {
        if (PacketSuppressor<MapRoomScanChanged>.IsSuppressed)
        {
            return;
        }

        if (!__instance.TryGetNitroxId(out NitroxId mapRoomId))
        {
            Log.Warn($"Could not find NitroxId on scanner room when changing scan target: {__instance.gameObject.GetFullHierarchyPath()}");
            return;
        }

        Resolve<IPacketSender>().Send(new MapRoomScanChanged(mapRoomId, newTypeToScan.ToString()));
    }
}
