using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PingManager_NotifyRename_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PingManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod(nameof(PingManager.NotifyRename), BindingFlags.Public | BindingFlags.Static);

        public static void Postfix(PingInstance instance)
        {
            if (!instance)
            {
                return;
            }
            NitroxServiceLocator.LocateService<IPacketSender>().Send(new PingRenamed(NitroxEntity.GetId(instance.gameObject), instance.GetLabel(), SerializationHelper.GetBytes(instance.gameObject)));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
