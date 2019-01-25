using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Base_ClearGeometry_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Base);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ClearGeometry", BindingFlags.Public | BindingFlags.Instance);

        /**
         * When new bases are constructed it will sometimes clear all of the pieces 
         * and reconnect them.  (This is primarily for visual purposes so it can change
         * out the model if required.)  When these pieces are cleared, we need to persist
         * them so that we can update the newly placed pieces with the proper id.  The new
         * pieces are added by Base.SpawnPiece (see that patch)
         */
        public static Dictionary<string, string> GuidByObjectKey = new Dictionary<string, string>();

        public static void Prefix(Base __instance)
        {
            if(__instance == null)
            {
                return;
            }

            Transform[] cellObjects = (Transform[] )__instance.ReflectionGet("cellObjects");

            if(cellObjects == null)
            {
                return;
            }

            foreach(Transform cellObject in cellObjects)
            {
                if(cellObject != null)
                {
                    for(int i = 0; i < cellObject.childCount; i++)
                    {
                        Transform child = cellObject.GetChild(i);

                        if (child != null && child.gameObject != null)
                        {
                            if(child.gameObject.GetComponent<UniqueIdentifier>() != null)
                            {
                                string guid = child.gameObject.GetComponent<UniqueIdentifier>().Id;
                                string key = getObjectKey(child.gameObject.name, child.position);
                                GuidByObjectKey[key] = guid;
                            }
                        }
                    }
                }
            }
        }

        public static string getObjectKey(string name, Vector3 postion)
        {
            return name + postion.ToString();
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
