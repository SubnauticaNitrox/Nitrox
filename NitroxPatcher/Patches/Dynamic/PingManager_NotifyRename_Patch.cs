using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
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
            NitroxId nitroxId = NitroxEntity.GetId(instance.gameObject);
            if (nitroxId == null)
            {
                Log.Error($"Missing Nitrox id on beacon: {instance.GetHierarchyPath()}");
                return;
            }
            
            PingRenamed packet = new PingRenamed(nitroxId, instance.GetLabel(), SerializationHelper.GetBytes(instance.gameObject));
            NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
