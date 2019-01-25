using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class ToggleLights_SetLightsActive_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ToggleLights);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetLightsActive", BindingFlags.Public | BindingFlags.Instance);

        private static readonly HashSet<Type> syncedParents = new HashSet<Type>()
        {
            typeof(SeaMoth),
            typeof(Seaglide),
            typeof(FlashLight),
            // LEDLight uses ToggleLights, but does not provide a method to toggle them.
            typeof(LEDLight)
        };

        public static bool Prefix(ToggleLights __instance, out bool __state)
        {
            __state = __instance.lightsActive;
            return true;
        }

        public static void Postfix(ToggleLights __instance, bool __state)
        {
            if (__state != __instance.lightsActive)
            {
                // Find the right gameobject in the hierarchy to sync on:
                GameObject gameObject = null;
                foreach (Type t in syncedParents)
                {
                    if (__instance.GetComponent(t))
                    {
                        gameObject = __instance.gameObject;
                        break;
                    }
                    else if (__instance.GetComponentInParent(t))
                    {
                        gameObject = __instance.transform.parent.gameObject;
                        break;
                    }
                }

                if (!gameObject)
                {
                    Log.Info("ToggleLights does not have a gameObject to sync on. Is this a new item?");
                    DebugUtils.PrintHierarchy(__instance.gameObject);
                }

                string guid = GuidHelper.GetGuid(gameObject);

                NitroxServiceLocator.LocateService<IPacketSender>().Send(new NitroxModel.Packets.ToggleLights(guid, __instance.lightsActive));
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }

        public class LightToggleContainer
        {
            public readonly Type ComponentType;
            public readonly bool InParent;

            public LightToggleContainer(Type componentType, bool inParent)
            {
                ComponentType = componentType;
                InParent = inParent;
            }

            public Component Get(GameObject go)
            {
                if (InParent)
                {
                    return go.GetComponentInParent(ComponentType);
                }

                return go.GetComponent(ComponentType);
            }
        }
    }
}
