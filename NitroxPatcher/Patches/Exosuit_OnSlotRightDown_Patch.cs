using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Exosuit_OnSlotRightDown_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitTorpedoArm);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Shoot", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(ExosuitTorpedoArm __instance, TorpedoType torpedoType, Transform siloTransform, bool verbose, PacketSuppressor<ItemContainerRemove> __state)
        {
            Log.Info("TORPEDO:" + torpedoType + siloTransform);
            __state = NitroxServiceLocator.LocateService<IPacketSender>().Suppress<ItemContainerRemove>();
            NitroxServiceLocator.LocateService<ExosuitModulesEvent>().BroadcastTorpedoLaunch(torpedoType, siloTransform, verbose, __instance);

            return true;
        }

        public static void Postfix(PacketSuppressor<ItemContainerRemove> __state)
        {
            if (__state != null)
            {
                __state.Dispose();
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

