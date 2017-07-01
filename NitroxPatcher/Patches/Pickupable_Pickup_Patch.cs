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
    [HarmonyPatch(typeof(Pickupable))]
    [HarmonyPatch("Pickup")]
    public class Pickupable_Pickup
    {
        [HarmonyPrefix]
        public static bool Prefix(Pickupable __instance)
        {
            Multiplayer.PacketSender.PickupItem(__instance.gameObject.transform.position, __instance.gameObject.transform.name, __instance.GetTechType().ToString());
            return true;
        }
    }
}

