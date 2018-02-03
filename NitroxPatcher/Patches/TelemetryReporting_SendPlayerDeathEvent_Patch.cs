using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class TelemetryReporting_SendPlayerDeathEvent_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(TelemetryReporting);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SendPlayerDeathEvent", BindingFlags.Public | BindingFlags.Static);

        public static void Postfix(UnityEngine.Object context, Vector3 position)
        {
            Multiplayer.Logic.Player.BroadcastDeath(position);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
