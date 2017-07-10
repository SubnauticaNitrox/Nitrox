using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Pickupable_Pickup_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Pickupable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Pickup");

        public static bool Prefix(Pickupable __instance)
        {
            Multiplayer.PacketSender.PickupItem(__instance.gameObject, __instance.GetTechType().ToString());
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

