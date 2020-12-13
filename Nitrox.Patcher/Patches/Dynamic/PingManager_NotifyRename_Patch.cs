using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Patcher.Patches.Dynamic
{
    public class PingManager_NotifyRename_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PingManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod(nameof(PingManager.NotifyRename), BindingFlags.Public | BindingFlags.Static);

        public static void Postfix(PingInstance instance)
        {
            // Only beacons are synced here (not mission, vehicle or other signals) because spawning is handled differently for non-droppable entities
            if (!instance || !instance.GetComponent<Beacon>())
            {
                return;
            }
            
            PingRenamed packet = new PingRenamed(NitroxEntity.GetId(instance.gameObject), instance.GetLabel(), SerializationHelper.GetBytes(instance.gameObject));
            NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
