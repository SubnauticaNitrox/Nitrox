using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PingManager_NotifyRename_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PingManager.NotifyRename(default(PingInstance)));

        public static void Postfix(PingInstance instance)
        {
            // Only beacons are synced here (not mission, vehicle or other signals) because spawning is handled differently for non-droppable entities
            if (!instance || !instance.GetComponent<Beacon>())
            {
                return;
            }

            PingRenamed packet = new(NitroxEntity.GetId(instance.gameObject), instance.GetLabel(), SerializationHelper.GetBytes(instance.gameObject));
            NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
