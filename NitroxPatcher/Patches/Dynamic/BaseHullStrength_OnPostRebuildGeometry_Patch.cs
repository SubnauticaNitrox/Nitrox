using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class BaseHullStrength_OnPostRebuildGeometry_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseHullStrength);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPostRebuildGeometry", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(BaseHullStrength __instance, Base b)
        {
            if (NitroxServiceLocator.LocateService<Building>().isInitialSyncing || NitroxServiceLocator.LocateService<Building>().remoteEventActive)
            {

                if (GameModeUtils.RequiresReinforcements())
                {
                    float num = 10f;
                    ((List<LiveMixin>)__instance.ReflectionGet("victims")).Clear();
                    foreach (Int3 cell in ((Base)__instance.ReflectionGet("baseComp")).AllCells)
                    {
                        if (((Base)__instance.ReflectionGet("baseComp")).GridToWorld(cell).y < 0f)
                        {
                            Transform cellObject = ((Base)__instance.ReflectionGet("baseComp")).GetCellObject(cell);
                            if (cellObject != null)
                            {
                                ((List<LiveMixin>)__instance.ReflectionGet("victims")).Add(cellObject.GetComponent<LiveMixin>());
                                num += ((Base)__instance.ReflectionGet("baseComp")).GetHullStrength(cell);
                            }
                        }
                    }
                    if (!UnityEngine.Mathf.Approximately(num, (float)__instance.ReflectionGet("totalStrength")))
                    {
                        if (!NitroxServiceLocator.LocateService<Building>().isInitialSyncing) // Display no Messages on Initialsync
                        {
                            // If remote player finished construction of a base structure, calculate the distance 
                            // and display the message only if remote player is near the lokal player.
                            // ## TODO BUILDING ##
                            // Calulate if near and then:
                            // ErrorMessage.AddMessage(Language.main.GetFormat<float, float>("BaseHullStrChanged", num - (float)instance.ReflectionGet("totalStrength"), num));
                        }
                    }
                    __instance.ReflectionSet("totalStrength", num);
                }
                return false;
            }
            return true;

            
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
