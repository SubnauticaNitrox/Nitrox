using System;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches
{
    public class Constructable_Deconstruct_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct");

        public static readonly OpCode CONSTRUCTION_COMPLETE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object CONSTRUCTION_COMPLETE_INJECTION_OPERAND = TARGET_CLASS.GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Constructable __instance)
        {
            CheckToCopyGuidToGhost(__instance);

            if (!__instance._constructed && __instance.constructedAmount > 0)
            {
                NitroxServiceLocator.LocateService<Building>().ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            }

            return true;
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount <= 0f)
            {
                NitroxServiceLocator.LocateService<Building>().DeconstructionComplete(__instance.gameObject);
            }
        }

        /**
         * On base pieces we need to have special logic during deconstruction to transfer the guid from the main 
         * object to the ghost.  This method will see if we have a stored LATEST_DECONSTRUCTED_BASE_PIECE_GUID 
         * (set from deconstructor patch).  If we do, then we'll copy the guid to the new ghost.  This is in 
         * amount changed as we don't currently have a good hook for 'startDeconstruction'; this method will
         * run its logic on the first amount changed instead - effectively the same thing.
         */
        public static void CheckToCopyGuidToGhost(Constructable __instance)
        {
            Optional<object> opGuid = TransientLocalObjectManager.Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID);

            if (opGuid.IsPresent())
            {
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, null);

                string guid = (string)opGuid.Get();
                Log.Info("Setting ghost guid " + guid);
                GuidHelper.SetNewGuid(__instance.gameObject, guid);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
