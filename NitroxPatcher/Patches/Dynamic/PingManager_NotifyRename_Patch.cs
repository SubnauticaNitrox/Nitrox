using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PingManager_NotifyRename_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PingManager.NotifyRename(default(PingInstance)));

    public static void Postfix(PingInstance instance)
    {
        // Only beacons are synced here (not mission, vehicle or other signals) because spawning is handled differently for non-droppable entities
        if (!instance || !instance.GetComponent<Beacon>())
        {
            return;
        }

        if (instance.TryGetIdOrWarn(out NitroxId id))
        {
            PingRenamed packet = new(id, instance.GetLabel(), SerializationHelper.GetBytes(instance.gameObject));
            Resolve<IPacketSender>().Send(packet);
        }
    }
}
